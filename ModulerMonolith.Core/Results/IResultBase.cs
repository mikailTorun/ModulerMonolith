namespace ModulerMonolith.Core.Results;

/// <summary>
/// Tüm Result tipleri için temel arayüz.
/// TransactionBehavior bu arayüz üzerinden IsSuccess'i okur.
/// </summary>
public interface IResultBase
{
    bool IsSuccess { get; }
    IReadOnlyList<ResultError> Errors { get; }
    IReadOnlyList<string> Messages { get; }
}

/// <summary>
/// Mediator'ın validation hatalarında TResponse.Fail(errors) çağırmasını
/// reflection olmadan mümkün kılan C# 11 static abstract interface.
/// </summary>
public interface IResultBase<TSelf> : IResultBase
    where TSelf : IResultBase<TSelf>
{
    static abstract TSelf Fail(IReadOnlyList<ResultError> errors);
}
