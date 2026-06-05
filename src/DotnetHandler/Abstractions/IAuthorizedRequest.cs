namespace DotnetHandler.Abstractions;

public interface IAuthorizedRequest
{
    IEnumerable<string> RequiredPermissions { get; }
}
