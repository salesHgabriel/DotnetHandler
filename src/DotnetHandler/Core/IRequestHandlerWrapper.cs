using DotnetHandler.Abstractions;

namespace DotnetHandler.Core;

internal interface IRequestHandlerWrapper { }

internal interface IRequestHandlerWrapper<TResponse> : IRequestHandlerWrapper
{
    Task<TResponse> HandleAsync(IRequest<TResponse> request, IServiceProvider services);
}
