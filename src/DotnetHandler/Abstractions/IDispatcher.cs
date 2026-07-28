namespace DotnetHandler.Abstractions;

public interface IDispatcher
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
}
