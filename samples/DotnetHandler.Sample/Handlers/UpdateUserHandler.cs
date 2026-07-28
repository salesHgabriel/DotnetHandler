using DotnetHandler.Abstractions;
using DotnetHandler.Sample.Data;
using DotnetHandler.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetHandler.Sample.Handlers;

public record UpdateUserCommand(Guid Id, string Name, string Email) : IRequest<UserResponse?>;

public class UpdateUserHandler(AppDbContext db) : IRequestHandler<UpdateUserCommand, UserResponse?>
{
    public async Task<UserResponse?> HandleAsync(UpdateUserCommand request, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            return null;

        user.Name = request.Name;
        user.Email = request.Email;
        await db.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Id, user.Name, user.Email);
    }
}
