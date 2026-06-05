using DotnetHandler.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace DotnetHandler.Tests.Behaviors;

public class CacheBehaviorTests
{
    private static IMemoryCache NewCache() => new MemoryCache(new MemoryCacheOptions());

    // ── Inline behavior implementation (mirrors the pattern in DotnetHandler.Sample) ──

    private class CacheBehavior<TRequest, TResponse>(IMemoryCache cache)
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

    // ── Fixtures ──

    private record CachableQuery(string CKey, string Result, TimeSpan? Dur = null)
        : IRequest<string>, ICacheableRequest<string>
    {
        public string CacheKey => CKey;
        public TimeSpan? CacheDuration => Dur;
    }

    private record PlainQuery(string Value) : IRequest<string>;

    // ── Tests ──

    [Fact]
    public async Task HandleAsync_WhenRequestNotCacheable_AlwaysCallsNext()
    {
        var behavior = new CacheBehavior<PlainQuery, string>(NewCache());
        var calls = 0;

        await behavior.HandleAsync(new PlainQuery("x"), () => { calls++; return Task.FromResult("x"); });
        await behavior.HandleAsync(new PlainQuery("x"), () => { calls++; return Task.FromResult("x"); });

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task HandleAsync_OnCacheMiss_CallsNextAndReturnsResult()
    {
        var behavior = new CacheBehavior<CachableQuery, string>(NewCache());
        var calls = 0;

        var result = await behavior.HandleAsync(
            new CachableQuery("k1", "hello"),
            () => { calls++; return Task.FromResult("hello"); });

        Assert.Equal(1, calls);
        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task HandleAsync_OnCacheHit_ReturnsCachedResultWithoutCallingNext()
    {
        var cache = NewCache();
        var behavior = new CacheBehavior<CachableQuery, string>(cache);
        var calls = 0;

        await behavior.HandleAsync(new CachableQuery("k2", "first"), () => { calls++; return Task.FromResult("first"); });
        var second = await behavior.HandleAsync(new CachableQuery("k2", "ignored"), () => { calls++; return Task.FromResult("ignored"); });

        Assert.Equal(1, calls);
        Assert.Equal("first", second);
    }

    [Fact]
    public async Task HandleAsync_DifferentCacheKeys_ExecuteHandlerEachTime()
    {
        var cache = NewCache();
        var behavior = new CacheBehavior<CachableQuery, string>(cache);
        var calls = 0;

        await behavior.HandleAsync(new CachableQuery("key-a", "a"), () => { calls++; return Task.FromResult("a"); });
        await behavior.HandleAsync(new CachableQuery("key-b", "b"), () => { calls++; return Task.FromResult("b"); });

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task HandleAsync_SameKeyAfterExpiry_CallsNextAgain()
    {
        var cache = NewCache();
        var behavior = new CacheBehavior<CachableQuery, string>(cache);

        await behavior.HandleAsync(new CachableQuery("k3", "v", TimeSpan.FromMilliseconds(1)), () => Task.FromResult("v"));

        await Task.Delay(50);

        var calls = 0;
        await behavior.HandleAsync(new CachableQuery("k3", "v2"), () => { calls++; return Task.FromResult("v2"); });

        Assert.Equal(1, calls);
    }
}
