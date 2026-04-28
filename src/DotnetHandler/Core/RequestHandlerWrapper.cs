using DotnetHandler.Abstractions;
using DotnetHandler.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetHandler.Core;

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : IRequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(IRequest<TResponse> request, IServiceProvider services)
    {
        var typed = (TRequest)request;
        var handler = services.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        if (handler is IValidationHandler<TRequest> vh)
        {
            var validationResult = await vh.ValidateAsync(typed);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }

        var behaviors = services.GetServices<IPipelineBehavior<TRequest, TResponse>>().ToList();

        Func<Task<TResponse>> next = () => handler.HandleAsync(typed);
        for (int i = behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var currentNext = next;
            next = () => behavior.HandleAsync(typed, currentNext);
        }

        return await next();
    }
}
