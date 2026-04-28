using DotnetHandler.Abstractions;
using DotnetHandler.Sample.Handlers;
using DotnetHandler.Validation;

namespace DotnetHandler.Sample.Http;

public static class UsersEndpoint
{
    public static void RegisterUsersEndpoints(this WebApplication app)
    {

        app.MapPost("/users", async (CreateUserCommand cmd, IDispatcher dispatcher) =>
        {
            try
            {
                var result = await dispatcher.Send(cmd);
                return Results.Created($"/users/{result.Id}", result);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { errors = ex.Errors });
            }
        });

        app.MapGet("/users", async (IDispatcher dispatcher) =>
            Results.Ok(await dispatcher.Send(new GetUsersQuery())));

        app.MapGet("/users/{id:guid}", async (Guid id, IDispatcher dispatcher) =>
        {
            var user = await dispatcher.Send(new GetUserQuery(id));
            return user is null ? Results.NotFound() : Results.Ok(user);
        });

        app.MapPut("/users/{id:guid}", async (Guid id, UpdateUserCommand cmd, IDispatcher dispatcher) =>
        {
            try
            {
                var result = await dispatcher.Send(cmd with { Id = id });
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { errors = ex.Errors });
            }
        });

        app.MapDelete("/users/{id:guid}", async (Guid id, IDispatcher dispatcher) =>
        {
            var deleted = await dispatcher.Send(new DeleteUserCommand(id));
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}