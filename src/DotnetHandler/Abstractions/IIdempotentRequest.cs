namespace DotnetHandler.Abstractions;

public interface IIdempotentRequest
{
    string IdempotencyKey { get; }
}
