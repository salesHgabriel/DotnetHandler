using DotnetHandler.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetHandler.Sample.Behaviors;

public class IdempotencyBehavior<TRequest, TResponse>(IMemoryCache cache)
    : IPipelineBehavior<TRequest, TResponse>
{
    private static readonly TimeSpan DefaultRetention = TimeSpan.FromHours(24);

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next)
    {
        if (request is not IIdempotentRequest idempotent || string.IsNullOrWhiteSpace(idempotent.IdempotencyKey))
            return await next();

        var key = $"idempotency:{idempotent.IdempotencyKey}";

        if (cache.TryGetValue(key, out TResponse? stored))
            return stored!;

        var result = await next();
        cache.Set(key, result, DefaultRetention);
        return result;
    }
}
