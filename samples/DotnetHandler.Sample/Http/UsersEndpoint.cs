using DotnetHandler.Abstractions;
using DotnetHandler.Authorization;
using DotnetHandler.Sample.Handlers;
using DotnetHandler.Validation;

namespace DotnetHandler.Sample.Http;

// Separate body record so the Idempotency-Key header doesn't bleed into JSON binding.
internal record CreateUserBody(string Name, string Email);

public static class UsersEndpoint
{
    public static void RegisterUsersEndpoints(this WebApplication app)
    {
        app.MapPost("/users", async (CreateUserBody body, IDispatcher dispatcher, HttpContext httpContext, CancellationToken cancellationToken) =>
        {
            var idempotencyKey = httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault() ?? string.Empty;
            var cmd = new CreateUserCommand(body.Name, body.Email, idempotencyKey);
            try
            {
                var result = await dispatcher.Send(cmd, cancellationToken);
                return Results.Created($"/users/{result.Id}", result);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { errors = ex.Errors });
            }
        });

        app.MapGet("/users", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
            Results.Ok(await dispatcher.Send(new GetUsersQuery(), cancellationToken)));

        app.MapGet("/users/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var user = await dispatcher.Send(new GetUserQuery(id), cancellationToken);
            return user is null ? Results.NotFound() : Results.Ok(user);
        });

        app.MapPut("/users/{id:guid}", async (Guid id, UpdateUserCommand cmd, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await dispatcher.Send(cmd with { Id = id }, cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { errors = ex.Errors });
            }
        });

        app.MapDelete("/users/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            try
            {
                var deleted = await dispatcher.Send(new DeleteUserCommand(id), cancellationToken);
                return deleted ? Results.NoContent() : Results.NotFound();
            }
            catch (UnauthorizedException ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
            }
        });
    }
}
