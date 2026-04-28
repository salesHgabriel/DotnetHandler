using DotnetHandler.Abstractions;
using DotnetHandler.Sample.Models;

namespace DotnetHandler.Sample.Events;

public record UserCreatedEvent(User User) : IEvent;
