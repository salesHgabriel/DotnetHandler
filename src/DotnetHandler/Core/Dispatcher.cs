using DotnetHandler.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetHandler.Core;

internal sealed class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _services;
    private readonly HandlerWrapperRegistry _registry;

    public Dispatcher(IServiceProvider services, HandlerWrapperRegistry registry)
    {
        _services = services;
        _registry = registry;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        var wrapper = _registry.GetWrapper<TResponse>(request.GetType());
        return wrapper.HandleAsync(request, _services);
    }

    public async Task Publish<TEvent>(TEvent @event) where TEvent : IEvent
    {
        var listeners = _services.GetServices<IEventListener<TEvent>>();
        foreach (var listener in listeners)
            await listener.HandleAsync(@event);
    }
}
