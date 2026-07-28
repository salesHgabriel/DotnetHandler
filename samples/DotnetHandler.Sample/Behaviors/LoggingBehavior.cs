using DotnetHandler.Abstractions;
using System.Diagnostics;

namespace DotnetHandler.Sample.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Handling {Request}", requestName);

        var result = await next(cancellationToken);

        _logger.LogInformation("Handled {Request} in {Elapsed}ms", requestName, sw.ElapsedMilliseconds);

        return result;
    }
}
