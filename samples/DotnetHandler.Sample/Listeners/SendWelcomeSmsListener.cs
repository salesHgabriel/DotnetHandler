using DotnetHandler.Abstractions;
using DotnetHandler.Sample.Events;

namespace DotnetHandler.Sample.Listeners;

public class SendWelcomeSmsListener : IEventListener<UserCreatedEvent>
{
    private readonly ILogger<SendWelcomeSmsListener> _logger;

    public SendWelcomeSmsListener(ILogger<SendWelcomeSmsListener> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(UserCreatedEvent @event)
    {
        _logger.LogInformation(
            "Welcome SMS sent to {Name} <{Email}>",
            @event.User.Name,
            @event.User.Email);

        return Task.CompletedTask;
    }
}