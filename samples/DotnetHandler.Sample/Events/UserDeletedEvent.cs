using DotnetHandler.Abstractions;

namespace DotnetHandler.Sample.Events;

public record UserDeletedEvent(Guid UserId, string Email) : IEvent;
