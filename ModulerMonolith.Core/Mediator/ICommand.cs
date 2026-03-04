using ModulerMonolith.Core.Results;

namespace ModulerMonolith.Core.Mediator;

/// <summary>
/// Yan etkisi olan işlem. TResponse her zaman Result veya Result&lt;T&gt; olmalı.
/// Bu constraint sayesinde ValidationBehavior ve TransactionBehavior
/// TResponse.Fail() ve IsSuccess'e erişebilir — sıfır reflection.
/// </summary>
public interface ICommand<TResponse> where TResponse : IResultBase<TResponse> { }
