namespace DotnetHandler.Abstractions;

public interface IPermissionContext
{
    bool HasPermission(string permission);
}
