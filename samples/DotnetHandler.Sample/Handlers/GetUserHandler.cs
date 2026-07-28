using DotnetHandler.Abstractions;
using DotnetHandler.Sample.Data;
using DotnetHandler.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetHandler.Sample.Handlers;

public record GetUserQuery(Guid Id) : IRequest<UserResponse?>, ICacheableRequest<UserResponse?>
{
    public string CacheKey => $"users:{Id}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}

public class GetUserHandler(AppDbContext db) : IRequestHandler<GetUserQuery, UserResponse?>
{
    public async Task<UserResponse?> HandleAsync(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        return user is null ? null : new UserResponse(user.Id, user.Name, user.Email);
    }
}
