using DotnetHandler.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetHandler.Registration;

public sealed class EventRegistration<TEvent> where TEvent : IEvent
{
    private readonly IServiceCollection _services;

    internal EventRegistration(IServiceCollection services)
    {
        _services = services;
    }

    public EventRegistration<TEvent> Subscribe<TListener>()
        where TListener : class, IEventListener<TEvent>
    {
        _services.AddScoped<IEventListener<TEvent>, TListener>();
        return this;
    }
}

public sealed class EventBuilder
{
    private readonly IServiceCollection _services;

    internal EventBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public EventRegistration<TEvent> Register<TEvent>() where TEvent : IEvent
    {
        return new EventRegistration<TEvent>(_services);
    }
}
