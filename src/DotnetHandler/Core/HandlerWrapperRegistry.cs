namespace DotnetHandler.Core;

internal sealed class HandlerWrapperRegistry
{
    private readonly Dictionary<Type, IRequestHandlerWrapper> _wrappers = new();

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
}
