using DotnetHandler.Abstractions;
using DotnetHandler.Core;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetHandler.Registration;

public sealed class HandlerRegistration<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceCollection _services;
    private readonly HandlerWrapperRegistry _registry;

    internal HandlerRegistration(IServiceCollection services, HandlerWrapperRegistry registry)
    {
        _services = services;
        _registry = registry;
    }

    public HandlerRegistration<TRequest, TResponse> HandledBy<THandler>()
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        _services.AddScoped<IRequestHandler<TRequest, TResponse>, THandler>();
        _registry.Register(typeof(TRequest), new RequestHandlerWrapper<TRequest, TResponse>());
        return this;
    }
}

public sealed class HandlerBuilder
{
    private readonly IServiceCollection _services;
    private readonly HandlerWrapperRegistry _registry;

    internal HandlerBuilder(IServiceCollection services, HandlerWrapperRegistry registry)
    {
        _services = services;
        _registry = registry;
    }

    public HandlerRegistration<TRequest, TResponse> Register<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>
    {
        return new HandlerRegistration<TRequest, TResponse>(_services, _registry);
    }
}
