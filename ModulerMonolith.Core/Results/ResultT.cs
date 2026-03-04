using System.Text.Json.Serialization;

namespace ModulerMonolith.Core.Results;

public sealed class Result<T> : IResultBase<Result<T>>
{
    private Result(T data, IReadOnlyList<string> messages)
    {
        IsSuccess = true;
        Data = data;
        ErrorType = ErrorType.None;
        Errors = [];
        Messages = messages;
    }

    private Result(ErrorType errorType, IReadOnlyList<ResultError> errors)
    {
        IsSuccess = false;
        Data = default;
        ErrorType = errorType;
        Errors = errors;
        Messages = [];
    }

    public bool IsSuccess { get; }
    public T? Data { get; }
    public IReadOnlyList<ResultError> Errors { get; }
    public IReadOnlyList<string> Messages { get; }

    [JsonIgnore]
    public ErrorType ErrorType { get; }

    public static Result<T> Ok(T data)                          => new(data, []);
    public static Result<T> Ok(T data, params string[] messages)=> new(data, messages);
    public static Result<T> Fail(string message)                => new(ErrorType.General,  [new(string.Empty, message)]);
    public static Result<T> NotFound(string message, string property = "") => new(ErrorType.NotFound, [new(property, message)]);
    public static Result<T> Conflict(string message)            => new(ErrorType.Conflict, [new(string.Empty, message)]);

    // Mediator tarafından validation hatalarında çağrılır
    static Result<T> IResultBase<Result<T>>.Fail(IReadOnlyList<ResultError> errors)
        => new(ErrorType.Validation, errors);

    public static implicit operator Result<T>(T value) => Ok(value);
}
