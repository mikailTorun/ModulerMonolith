using System.Text.Json.Serialization;

namespace ModulerMonolith.Core.Results;

public sealed class Result : IResultBase<Result>
{
    private Result(bool isSuccess, ErrorType errorType, IReadOnlyList<ResultError> errors, IReadOnlyList<string> messages)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        Errors = errors;
        Messages = messages;
    }

    public bool IsSuccess { get; }
    public IReadOnlyList<ResultError> Errors { get; }
    public IReadOnlyList<string> Messages { get; }

    [JsonIgnore]
    public ErrorType ErrorType { get; }

    public static Result Ok()                           => new(true,  ErrorType.None,     [], []);
    public static Result Ok(params string[] messages)   => new(true,  ErrorType.None,     [], messages);
    public static Result Fail(string message)           => new(false, ErrorType.General,  [new(string.Empty, message)], []);
    public static Result NotFound(string message)       => new(false, ErrorType.NotFound, [new(string.Empty, message)], []);
    public static Result Conflict(string message)       => new(false, ErrorType.Conflict, [new(string.Empty, message)], []);

    // Validation hataları için — hem Mediator hem ValidationExceptionHandler kullanır
    public static Result Fail(IReadOnlyList<ResultError> errors)
        => new(false, ErrorType.Validation, errors, []);

    static Result IResultBase<Result>.Fail(IReadOnlyList<ResultError> errors)
        => Fail(errors);
}
