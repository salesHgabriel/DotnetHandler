using DotnetHandler.Abstractions;
using DotnetHandler.Sample.Data;
using DotnetHandler.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetHandler.Sample.Handlers;

public record GetUserQuery(Guid Id) : IRequest<UserResponse?>;

public class GetUserHandler(AppDbContext db) : IRequestHandler<GetUserQuery, UserResponse?>
{
    public async Task<UserResponse?> HandleAsync(GetUserQuery request)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.Id);
        return user is null ? null : new UserResponse(user.Id, user.Name, user.Email);
    }
}
