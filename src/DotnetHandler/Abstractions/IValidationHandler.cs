using DotnetHandler.Validation;

namespace DotnetHandler.Abstractions;

public interface IValidationHandler<TRequest>
{
    Task<ValidationResult> ValidateAsync(TRequest request);
}
