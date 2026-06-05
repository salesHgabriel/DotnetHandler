namespace DotnetHandler.Authorization;

public class UnauthorizedException : Exception
{
    public string Permission { get; }

    public UnauthorizedException(string permission)
        : base($"Missing required permission: '{permission}'.")
    {
        Permission = permission;
    }
}
