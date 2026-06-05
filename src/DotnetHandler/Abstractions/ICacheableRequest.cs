namespace DotnetHandler.Abstractions;

public interface ICacheableRequest<TResponse>
{
    string CacheKey { get; }
    TimeSpan? CacheDuration { get; }
}
