using DotnetHandler.Abstractions;
using DotnetHandler.Validation;

namespace DotnetHandler.Tests.Helpers;

// --- Shared request/handler ---
public record PingRequest(string Message) : IRequest<string>;

public class PingHandler : IRequestHandler<PingRequest, string>
{
    public Task<string> HandleAsync(PingRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult($"Pong: {request.Message}");
}

// --- Validation fixtures ---
public record CreateItemCommand(string Name) : IRequest<string>;

public class CreateItemHandler
    : IRequestHandler<CreateItemCommand, string>,
      IValidationHandler<CreateItemCommand>
{
    public static bool HandlerWasCalled { get; set; }

    public Task<ValidationResult> ValidateAsync(CreateItemCommand request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Task.FromResult(ValidationResult.Failure("Name is required"));

        return Task.FromResult(ValidationResult.Success());
    }

    public Task<string> HandleAsync(CreateItemCommand request, CancellationToken cancellationToken = default)
    {
        HandlerWasCalled = true;
        return Task.FromResult($"Created: {request.Name}");
    }
}

// --- Pipeline fixtures ---
public record EchoRequest(string Value) : IRequest<string>;

public class EchoHandler : IRequestHandler<EchoRequest, string>
{
    public Task<string> HandleAsync(EchoRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(request.Value);
}

public class RecordingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public static readonly List<string> Log = new();

    public async Task<TResponse> HandleAsync(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken = default)
    {
        Log.Add("before");
        var result = await next(cancellationToken);
        Log.Add("after");
        return result;
    }
}

public class PrefixBehavior : IPipelineBehavior<EchoRequest, string>
{
    public async Task<string> HandleAsync(EchoRequest request, Func<CancellationToken, Task<string>> next, CancellationToken cancellationToken = default)
    {
        var result = await next(cancellationToken);
        return $"[PREFIX]{result}";
    }
}

public class SuffixBehavior : IPipelineBehavior<EchoRequest, string>
{
    public async Task<string> HandleAsync(EchoRequest request, Func<CancellationToken, Task<string>> next, CancellationToken cancellationToken = default)
    {
        var result = await next(cancellationToken);
        return $"{result}[SUFFIX]";
    }
}

// --- Event fixtures ---
public record UserRegisteredEvent(string Username) : IEvent;

public class EmailListener : IEventListener<UserRegisteredEvent>
{
    public static readonly List<string> Received = new();

    public Task HandleAsync(UserRegisteredEvent @event, CancellationToken cancellationToken = default)
    {
        Received.Add($"email:{@event.Username}");
        return Task.CompletedTask;
    }
}

public class AuditListener : IEventListener<UserRegisteredEvent>
{
    public static readonly List<string> Received = new();

    public Task HandleAsync(UserRegisteredEvent @event, CancellationToken cancellationToken = default)
    {
        Received.Add($"audit:{@event.Username}");
        return Task.CompletedTask;
    }
}

public record UnhandledEvent(string Data) : IEvent;
