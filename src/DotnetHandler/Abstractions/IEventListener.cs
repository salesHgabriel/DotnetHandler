namespace DotnetHandler.Abstractions;

public interface IEventListener<TEvent>
{
    Task HandleAsync(TEvent @event);
}
