using DotnetHandler.Abstractions;
using DotnetHandler.Sample.Events;

namespace DotnetHandler.Sample.Listeners;

public class AuditUserDeletedListener(ILogger<AuditUserDeletedListener> logger)
    : IEventListener<UserDeletedEvent>
{
    public Task HandleAsync(UserDeletedEvent @event)
    {
        logger.LogInformation("User deleted — Id: {UserId}, Email: {Email}", @event.UserId, @event.Email);
        return Task.CompletedTask;
    }
}
