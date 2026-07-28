using DotnetHandler.Abstractions;
using DotnetHandler.Authorization;
using Xunit;

namespace DotnetHandler.Tests.Behaviors;

public class PermissionBehaviorTests
{
    // ── Inline behavior implementation (mirrors the pattern in DotnetHandler.Sample) ──

    private class PermissionBehavior<TRequest, TResponse>(IPermissionContext context)
        : IPipelineBehavior<TRequest, TResponse>
    {
        public async Task<TResponse> HandleAsync(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken = default)
        {
            if (request is not IAuthorizedRequest authorized)
                return await next(cancellationToken);

            foreach (var permission in authorized.RequiredPermissions)
            {
                if (!context.HasPermission(permission))
                    throw new UnauthorizedException(permission);
            }

            return await next(cancellationToken);
        }
    }

    // ── Fixtures ──

    private record SecuredRequest(string[] Permissions) : IRequest<string>, IAuthorizedRequest
    {
        public IEnumerable<string> RequiredPermissions => Permissions;
    }

    private record OpenRequest : IRequest<string>;

    private class StubPermissionContext(params string[] granted) : IPermissionContext
    {
        public bool HasPermission(string permission) =>
            granted.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    // ── Tests ──

    [Fact]
    public async Task HandleAsync_WhenRequestNotAuthorized_CallsNext()
    {
        var behavior = new PermissionBehavior<OpenRequest, string>(new StubPermissionContext());
        var calls = 0;

        await behavior.HandleAsync(new OpenRequest(), _ => { calls++; return Task.FromResult("ok"); });

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasRequiredPermission_ExecutesHandler()
    {
        var behavior = new PermissionBehavior<SecuredRequest, string>(new StubPermissionContext("users:read"));
        var calls = 0;

        var result = await behavior.HandleAsync(
            new SecuredRequest(["users:read"]),
            _ => { calls++; return Task.FromResult("ok"); });

        Assert.Equal(1, calls);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task HandleAsync_WhenUserMissingPermission_ThrowsUnauthorizedException()
    {
        var behavior = new PermissionBehavior<SecuredRequest, string>(new StubPermissionContext());

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            behavior.HandleAsync(new SecuredRequest(["users:delete"]), _ => Task.FromResult("ok")));
    }

    [Fact]
    public async Task HandleAsync_WhenOnePermissionMissing_ExceptionContainsMissingPermission()
    {
        var behavior = new PermissionBehavior<SecuredRequest, string>(new StubPermissionContext("users:read"));

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            behavior.HandleAsync(
                new SecuredRequest(["users:read", "users:delete"]),
                _ => Task.FromResult("ok")));

        Assert.Equal("users:delete", ex.Permission);
    }

    [Fact]
    public async Task HandleAsync_WhenAllPermissionsGranted_CallsNext()
    {
        var behavior = new PermissionBehavior<SecuredRequest, string>(
            new StubPermissionContext("users:read", "users:write"));
        var calls = 0;

        await behavior.HandleAsync(
            new SecuredRequest(["users:read", "users:write"]),
            _ => { calls++; return Task.FromResult("ok"); });

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task HandleAsync_WhenNoPermissionsRequired_CallsNext()
    {
        var behavior = new PermissionBehavior<SecuredRequest, string>(new StubPermissionContext());
        var calls = 0;

        await behavior.HandleAsync(
            new SecuredRequest([]),
            _ => { calls++; return Task.FromResult("ok"); });

        Assert.Equal(1, calls);
    }
}
