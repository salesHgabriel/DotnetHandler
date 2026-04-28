using DotnetHandler.Abstractions;
using FluentValidation;
using HandlerValidationException = DotnetHandler.Validation.ValidationException;

namespace DotnetHandler.Sample.Behaviors;

public class FluentValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next)
    {
        var errors = new List<string>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid)
                errors.AddRange(result.Errors.Select(e => e.ErrorMessage));
        }

        if (errors.Count > 0)
            throw new HandlerValidationException(errors.ToArray());

        return await next();
    }
}
