namespace DotnetHandler.Abstractions;

public interface IDispatcher
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request);
    Task Publish<TEvent>(TEvent @event) where TEvent : IEvent;
}
