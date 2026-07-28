using DotnetHandler.Abstractions;
using DotnetHandler.Authorization;

namespace DotnetHandler.Sample.Behaviors;

public class PermissionBehavior<TRequest, TResponse>(IPermissionContext permissionContext)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken = default)
    {
        if (request is not IAuthorizedRequest authorizedRequest)
            return await next(cancellationToken);

        foreach (var permission in authorizedRequest.RequiredPermissions)
        {
            if (!permissionContext.HasPermission(permission))
                throw new UnauthorizedException(permission);
        }

        return await next(cancellationToken);
    }
}
