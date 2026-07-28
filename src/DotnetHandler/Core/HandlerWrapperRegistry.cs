namespace DotnetHandler.Core;

internal sealed class HandlerWrapperRegistry
{
    private readonly Dictionary<Type, IRequestHandlerWrapper> _wrappers = new();
    private readonly HashSet<(Type Service, Type Implementation)> _scanned = new();

    internal void Register(Type requestType, IRequestHandlerWrapper wrapper)
    {
        _wrappers[requestType] = wrapper;
    }

    internal IRequestHandlerWrapper<TResponse> GetWrapper<TResponse>(Type requestType)
    {
        if (_wrappers.TryGetValue(requestType, out var wrapper))
            return (IRequestHandlerWrapper<TResponse>)wrapper;

        throw new InvalidOperationException(
            $"No handler registered for request type '{requestType.Name}'.");
    }

    /// <summary>Records that (service, implementation) was scanned on this registry; returns false if already recorded.</summary>
    internal bool TryMarkScanned(Type service, Type implementation)
    {
        return _scanned.Add((service, implementation));
    }
}
