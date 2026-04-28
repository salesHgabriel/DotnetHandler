using DotnetHandler.Abstractions;
using DotnetHandler.Sample.Data;
using DotnetHandler.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetHandler.Sample.Handlers;

public record GetUsersQuery : IRequest<List<UserResponse>>;

public class GetUsersHandler(AppDbContext db) : IRequestHandler<GetUsersQuery, List<UserResponse>>
{
    public async Task<List<UserResponse>> HandleAsync(GetUsersQuery request)
    {
        return await db.Users
            .AsNoTracking()
            .Select(u => new UserResponse(u.Id, u.Name, u.Email))
            .ToListAsync();
    }
}
