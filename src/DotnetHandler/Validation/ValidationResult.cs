namespace DotnetHandler.Validation;

public class ValidationResult
{
    public bool IsValid => !Errors.Any();

    public List<string> Errors { get; } = new();

    public static ValidationResult Success() => new();

    public static ValidationResult Failure(params string[] errors)
    {
        var result = new ValidationResult();
        result.Errors.AddRange(errors);
        return result;
    }
}
