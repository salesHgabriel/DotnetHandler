using DotnetHandler.Abstractions;

namespace DotnetHandler.Sample.Services;

// Reads granted permissions from the X-Permissions header (comma-separated).
// Replace with claims-based logic in production.
public class CurrentUserPermissionContext(IHttpContextAccessor httpContextAccessor) : IPermissionContext
{
    public bool HasPermission(string permission)
    {
        var header = httpContextAccessor.HttpContext?.Request.Headers["X-Permissions"].ToString();
        if (string.IsNullOrEmpty(header))
            return false;

        return header
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}
