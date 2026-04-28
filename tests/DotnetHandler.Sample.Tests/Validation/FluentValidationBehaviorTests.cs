using DotnetHandler.Abstractions;
using DotnetHandler.Registration;
using Microsoft.EntityFrameworkCore;
using DotnetHandler.Sample.Tests.Helpers;
using DotnetHandler.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using HandlerValidationException = DotnetHandler.Validation.ValidationException;

namespace DotnetHandler.Sample.Tests.Validation;

public class FluentValidationBehaviorTests
{
    [Fact]
    public async Task Passes_through_when_no_validator_registered()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(opt =>
            opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddDotnetHandler(app =>
        {
            app.Handlers(h =>
                h.Register<CreateUserCommand, UserResponse>().HandledBy<CreateUserHandler>());
            app.Pipeline(p => p.Use(typeof(FluentValidationBehavior<,>)));
        });

        var sp = services.BuildServiceProvider();
        var dispatcher = sp.CreateScope().ServiceProvider.GetRequiredService<IDispatcher>();

        var result = await dispatcher.Send(new CreateUserCommand("", "not-an-email"));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Collects_all_errors_from_single_validator()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var ex = await Assert.ThrowsAsync<HandlerValidationException>(
            () => dispatcher.Send(new CreateUserCommand("", "bad")));

        Assert.Contains("Name is required.", ex.Errors);
        Assert.Contains("A valid email address is required.", ex.Errors);
    }

    [Fact]
    public async Task Collects_errors_from_multiple_validators()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(opt =>
            opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
        services.AddScoped<IValidator<CreateUserCommand>, ExtraCreateUserValidator>();

        services.AddDotnetHandler(app =>
        {
            app.Handlers(h =>
                h.Register<CreateUserCommand, UserResponse>().HandledBy<CreateUserHandler>());
            app.Pipeline(p => p.Use(typeof(FluentValidationBehavior<,>)));
        });

        var sp = services.BuildServiceProvider();
        var dispatcher = sp.CreateScope().ServiceProvider.GetRequiredService<IDispatcher>();

        var ex = await Assert.ThrowsAsync<HandlerValidationException>(
            () => dispatcher.Send(new CreateUserCommand("", "bad")));

        Assert.Contains("Name is required.", ex.Errors);
        Assert.Contains("Extra: name cannot be empty.", ex.Errors);
    }

    [Fact]
    public async Task Does_not_throw_when_all_validators_pass()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var result = await dispatcher.Send(new CreateUserCommand("Valid Name", "valid@example.com"));

        Assert.NotNull(result);
        Assert.Equal("Valid Name", result.Name);
    }

    [Fact]
    public async Task Handler_is_not_called_when_validation_fails()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(opt =>
            opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
        services.AddDotnetHandler(app =>
        {
            app.Handlers(h =>
                h.Register<CreateUserCommand, UserResponse>().HandledBy<TrackingCreateUserHandler>());
            app.Pipeline(p => p.Use(typeof(FluentValidationBehavior<,>)));
        });
        var sp = services.BuildServiceProvider();
        TrackingCreateUserHandler.CallCount = 0;

        var dispatcher = sp.CreateScope().ServiceProvider.GetRequiredService<IDispatcher>();
        await Assert.ThrowsAsync<HandlerValidationException>(
            () => dispatcher.Send(new CreateUserCommand("", "bad")));

        Assert.Equal(0, TrackingCreateUserHandler.CallCount);
    }
}

// Extra validator for multi-validator test
public class ExtraCreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public ExtraCreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Extra: name cannot be empty.");
    }
}

// Handler that tracks invocation count
public class TrackingCreateUserHandler(TestDbContext db) : IRequestHandler<CreateUserCommand, UserResponse>
{
    public static int CallCount { get; set; }

    public async Task<UserResponse> HandleAsync(CreateUserCommand request)
    {
        CallCount++;
        var user = new User { Name = request.Name, Email = request.Email };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return new UserResponse(user.Id, user.Name, user.Email);
    }
}
