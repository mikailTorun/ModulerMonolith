using Microsoft.AspNetCore.Http;
using ModulerMonolith.Core.Results;

namespace ModulerMonolith.Infrastructure.Http;

/// <summary>
/// Tüm yanıtlar aynı şemada döner: { isSuccess, data, errors, messages }
/// </summary>
public static class ResultExtensions
{
    public static IResult ToApiResult<T>(this Result<T> result, int successCode = StatusCodes.Status200OK) =>
        Results.Json(result, statusCode: result.IsSuccess ? successCode : MapStatusCode(result.ErrorType));

    public static IResult ToApiResult(this Result result, int successCode = StatusCodes.Status200OK) =>
        Results.Json(result, statusCode: result.IsSuccess ? successCode : MapStatusCode(result.ErrorType));

    private static int MapStatusCode(ErrorType type) => type switch
    {
        ErrorType.NotFound     => StatusCodes.Status404NotFound,
        ErrorType.Conflict     => StatusCodes.Status409Conflict,
        ErrorType.Validation   => StatusCodes.Status422UnprocessableEntity,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        _                      => StatusCodes.Status400BadRequest
    };
}
