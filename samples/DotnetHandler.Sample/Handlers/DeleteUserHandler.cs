using DotnetHandler.Abstractions;
using DotnetHandler.Sample.Data;
using DotnetHandler.Sample.Events;
using Microsoft.EntityFrameworkCore;

namespace DotnetHandler.Sample.Handlers;

public record DeleteUserCommand(Guid Id) : IRequest<bool>;

public class DeleteUserHandler(AppDbContext db, IDispatcher dispatcher)
    : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteUserCommand request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
        if (user is null)
            return false;

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        await dispatcher.Publish(new UserDeletedEvent(user.Id, user.Email));

        return true;
    }
}
