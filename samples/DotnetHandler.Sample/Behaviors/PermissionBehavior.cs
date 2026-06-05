using DotnetHandler.Abstractions;
using DotnetHandler.Authorization;

namespace DotnetHandler.Sample.Behaviors;

public class PermissionBehavior<TRequest, TResponse>(IPermissionContext permissionContext)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next)
    {
        if (request is not IAuthorizedRequest authorizedRequest)
            return await next();

        foreach (var permission in authorizedRequest.RequiredPermissions)
        {
            if (!permissionContext.HasPermission(permission))
                throw new UnauthorizedException(permission);
        }

        return await next();
    }
}
