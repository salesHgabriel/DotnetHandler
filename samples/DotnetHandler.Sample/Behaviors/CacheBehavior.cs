using DotnetHandler.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetHandler.Sample.Behaviors;

public class CacheBehavior<TRequest, TResponse>(IMemoryCache cache)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next)
    {
        if (request is not ICacheableRequest<TResponse> cacheable)
            return await next();

        if (cache.TryGetValue(cacheable.CacheKey, out TResponse? cached))
            return cached!;

        var result = await next();
        cache.Set(cacheable.CacheKey, result, cacheable.CacheDuration ?? TimeSpan.FromMinutes(5));
        return result;
    }
}
