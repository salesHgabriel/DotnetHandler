using DotnetHandler.Abstractions;
using DotnetHandler.Generated;
using DotnetHandler.Registration;
using DotnetHandler.Validation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HandlerValidationException = DotnetHandler.Validation.ValidationException;

namespace DotnetHandler.Sample.Tests.Helpers;

// ── Domain ─────────────────────────────────────────────────────────────────

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public record UserResponse(Guid Id, string Name, string Email);

// ── DbContext ───────────────────────────────────────────────────────────────

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
}

// ── Commands / Queries ──────────────────────────────────────────────────────

public record CreateUserCommand(string Name, string Email) : IRequest<UserResponse>;
public record GetUserQuery(Guid Id) : IRequest<UserResponse?>;
public record GetUsersQuery : IRequest<List<UserResponse>>;
public record UpdateUserCommand(Guid Id, string Name, string Email) : IRequest<UserResponse?>;
public record DeleteUserCommand(Guid Id) : IRequest<bool>;

// ── Handlers ────────────────────────────────────────────────────────────────

public class CreateUserHandler(TestDbContext db) : IRequestHandler<CreateUserCommand, UserResponse>
{
    public async Task<UserResponse> HandleAsync(CreateUserCommand request)
    {
        var user = new User { Name = request.Name, Email = request.Email };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return new UserResponse(user.Id, user.Name, user.Email);
    }
}

public class GetUserHandler(TestDbContext db) : IRequestHandler<GetUserQuery, UserResponse?>
{
    public async Task<UserResponse?> HandleAsync(GetUserQuery request)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.Id);
        return user is null ? null : new UserResponse(user.Id, user.Name, user.Email);
    }
}

public class GetUsersHandler(TestDbContext db) : IRequestHandler<GetUsersQuery, List<UserResponse>>
{
    public async Task<List<UserResponse>> HandleAsync(GetUsersQuery _) =>
        await db.Users.AsNoTracking()
            .Select(u => new UserResponse(u.Id, u.Name, u.Email))
            .ToListAsync();
}

public class UpdateUserHandler(TestDbContext db) : IRequestHandler<UpdateUserCommand, UserResponse?>
{
    public async Task<UserResponse?> HandleAsync(UpdateUserCommand request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
        if (user is null) return null;

        user.Name = request.Name;
        user.Email = request.Email;
        await db.SaveChangesAsync();
        return new UserResponse(user.Id, user.Name, user.Email);
    }
}

public class DeleteUserHandler(TestDbContext db) : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteUserCommand request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
        if (user is null) return false;

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return true;
    }
}

// ── Validators ──────────────────────────────────────────────────────────────

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}

// ── FluentValidation pipeline behavior ─────────────────────────────────────

public class FluentValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next)
    {
        var errors = new List<string>();
        foreach (var v in validators)
        {
            var result = await v.ValidateAsync(request);
            if (!result.IsValid)
                errors.AddRange(result.Errors.Select(e => e.ErrorMessage));
        }

        if (errors.Count > 0)
            throw new HandlerValidationException(errors.ToArray());

        return await next();
    }
}

// ── Service factory ─────────────────────────────────────────────────────────

public static class SampleServiceFactory
{
    public static IServiceProvider Build(Action<IServiceCollection>? extra = null)
    {
        var services = new ServiceCollection();

        services.AddDbContext<TestDbContext>(opt =>
            opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
        services.AddScoped<IValidator<UpdateUserCommand>, UpdateUserCommandValidator>();

        services.AddDotnetHandler(app =>
        {
            app.UseGeneratedHandlers();
            app.Pipeline(p => p.Use(typeof(FluentValidationBehavior<,>)));
        });

        extra?.Invoke(services);
        return services.BuildServiceProvider();
    }

    public static IDispatcher Dispatcher(IServiceProvider sp) =>
        sp.CreateScope().ServiceProvider.GetRequiredService<IDispatcher>();
}
