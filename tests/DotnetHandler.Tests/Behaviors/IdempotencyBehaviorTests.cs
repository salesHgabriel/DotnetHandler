using DotnetHandler.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace DotnetHandler.Tests.Behaviors;

public class IdempotencyBehaviorTests
{
    private static IMemoryCache NewCache() => new MemoryCache(new MemoryCacheOptions());

    // ── Inline behavior implementation (mirrors the pattern in DotnetHandler.Sample) ──

    private class IdempotencyBehavior<TRequest, TResponse>(IMemoryCache cache)
        : IPipelineBehavior<TRequest, TResponse>
    {
        public async Task<TResponse> HandleAsync(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken = default)
        {
            if (request is not IIdempotentRequest idempotent || string.IsNullOrWhiteSpace(idempotent.IdempotencyKey))
                return await next(cancellationToken);

            var key = $"idempotency:{idempotent.IdempotencyKey}";
            if (cache.TryGetValue(key, out TResponse? stored))
                return stored!;

            var result = await next(cancellationToken);
            cache.Set(key, result, TimeSpan.FromHours(24));
            return result;
        }
    }

    // ── Fixtures ──

    private record IdempotentCommand(string IdempotencyKey, string Value)
        : IRequest<string>, IIdempotentRequest;

    private record PlainCommand(string Value) : IRequest<string>;

    // ── Tests ──

    [Fact]
    public async Task HandleAsync_WhenRequestNotIdempotent_AlwaysCallsNext()
    {
        var behavior = new IdempotencyBehavior<PlainCommand, string>(NewCache());
        var calls = 0;

        await behavior.HandleAsync(new PlainCommand("x"), _ => { calls++; return Task.FromResult("x"); });
        await behavior.HandleAsync(new PlainCommand("x"), _ => { calls++; return Task.FromResult("x"); });

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task HandleAsync_WhenIdempotencyKeyIsEmpty_AlwaysCallsNext()
    {
        var behavior = new IdempotencyBehavior<IdempotentCommand, string>(NewCache());
        var calls = 0;

        await behavior.HandleAsync(new IdempotentCommand("", "x"), _ => { calls++; return Task.FromResult("x"); });
        await behavior.HandleAsync(new IdempotentCommand("", "x"), _ => { calls++; return Task.FromResult("x"); });

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task HandleAsync_WhenNewKey_CallsNextAndReturnsResult()
    {
        var behavior = new IdempotencyBehavior<IdempotentCommand, string>(NewCache());
        var calls = 0;

        var result = await behavior.HandleAsync(
            new IdempotentCommand("key-1", "created"),
            _ => { calls++; return Task.FromResult("created"); });

        Assert.Equal(1, calls);
        Assert.Equal("created", result);
    }

    [Fact]
    public async Task HandleAsync_WhenDuplicateKey_ReturnsCachedResultWithoutCallingNext()
    {
        var cache = NewCache();
        var behavior = new IdempotencyBehavior<IdempotentCommand, string>(cache);
        var calls = 0;

        await behavior.HandleAsync(new IdempotentCommand("key-2", "first"), _ => { calls++; return Task.FromResult("first"); });
        var second = await behavior.HandleAsync(new IdempotentCommand("key-2", "second"), _ => { calls++; return Task.FromResult("second"); });

        Assert.Equal(1, calls);
        Assert.Equal("first", second);
    }

    [Fact]
    public async Task HandleAsync_DifferentKeys_BothCallNext()
    {
        var cache = NewCache();
        var behavior = new IdempotencyBehavior<IdempotentCommand, string>(cache);
        var calls = 0;

        await behavior.HandleAsync(new IdempotentCommand("key-a", "a"), _ => { calls++; return Task.FromResult("a"); });
        await behavior.HandleAsync(new IdempotentCommand("key-b", "b"), _ => { calls++; return Task.FromResult("b"); });

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task HandleAsync_WhenKeyIsWhitespace_AlwaysCallsNext()
    {
        var behavior = new IdempotencyBehavior<IdempotentCommand, string>(NewCache());
        var calls = 0;

        await behavior.HandleAsync(new IdempotentCommand("   ", "x"), _ => { calls++; return Task.FromResult("x"); });
        await behavior.HandleAsync(new IdempotentCommand("   ", "x"), _ => { calls++; return Task.FromResult("x"); });

        Assert.Equal(2, calls);
    }
}
