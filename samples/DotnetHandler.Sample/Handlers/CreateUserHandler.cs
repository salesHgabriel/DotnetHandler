using DotnetHandler.Abstractions;
using DotnetHandler.Sample.Data;
using DotnetHandler.Sample.Events;
using DotnetHandler.Sample.Models;

namespace DotnetHandler.Sample.Handlers;

public record CreateUserCommand(string Name, string Email) : IRequest<UserResponse>;

public class CreateUserHandler(AppDbContext db, IDispatcher dispatcher)
    : IRequestHandler<CreateUserCommand, UserResponse>
{
    public async Task<UserResponse> HandleAsync(CreateUserCommand request)
    {
        var user = new User { Name = request.Name, Email = request.Email };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        await dispatcher.Publish(new UserCreatedEvent(user));

        return new UserResponse(user.Id, user.Name, user.Email);
    }
}
