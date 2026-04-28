using DotnetHandler.Abstractions;
using DotnetHandler.Registration;
using DotnetHandler.Sample.Behaviors;
using DotnetHandler.Sample.Data;
using DotnetHandler.Sample.Events;
using DotnetHandler.Sample.Handlers;
using DotnetHandler.Sample.Http;
using DotnetHandler.Sample.Listeners;
using DotnetHandler.Sample.Models;
using DotnetHandler.Sample.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=users.db"));

builder.Services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
builder.Services.AddScoped<IValidator<UpdateUserCommand>, UpdateUserCommandValidator>();

builder.Services.AddDotnetHandler(app =>
{
    app.Handlers(h =>
    {
        h.Register<CreateUserCommand, UserResponse>().HandledBy<CreateUserHandler>();
        h.Register<GetUserQuery, UserResponse?>().HandledBy<GetUserHandler>();
        h.Register<GetUsersQuery, List<UserResponse>>().HandledBy<GetUsersHandler>();
        h.Register<UpdateUserCommand, UserResponse?>().HandledBy<UpdateUserHandler>();
        h.Register<DeleteUserCommand, bool>().HandledBy<DeleteUserHandler>();
    });

    app.Events(e =>
    {
        e.Register<UserCreatedEvent>()
            .Subscribe<SendWelcomeEmailListener>()
            .Subscribe<SendWelcomeSmsListener>();

        e.Register<UserDeletedEvent>()
            .Subscribe<AuditUserDeletedListener>();
    });

    app.Pipeline(p =>
        p.Use(typeof(LoggingBehavior<,>))
         .Use(typeof(FluentValidationBehavior<,>)));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    await dbContext.Database.MigrateAsync();
}

app.RegisterUsersEndpoints();

app.Run();
