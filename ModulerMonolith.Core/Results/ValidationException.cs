namespace ModulerMonolith.Core.Results;

/// <summary>
/// Query validation hatalarında fırlatılır → ValidationExceptionHandler 422 döner.
/// Command'larda exception fırlatılmaz — Result.Fail() döndürülür.
/// </summary>
public sealed class ValidationException(IReadOnlyList<ResultError> errors)
    : Exception("One or more validation errors occurred.")
{
    public IReadOnlyList<ResultError> Errors { get; } = errors;
}
