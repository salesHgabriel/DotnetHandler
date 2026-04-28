using DotnetHandler.Abstractions;
using DotnetHandler.Sample.Events;

namespace DotnetHandler.Sample.Listeners
{
    public class SendWelcomeEmailListener : IEventListener<UserCreatedEvent>
    {
        private readonly ILogger<SendWelcomeEmailListener> _logger;

        public SendWelcomeEmailListener(ILogger<SendWelcomeEmailListener> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(UserCreatedEvent @event)
        {
            _logger.LogInformation(
                "Welcome email sent to {Name} <{Email}>",
                @event.User.Name,
                @event.User.Email);

            return Task.CompletedTask;
        }
    }
}
