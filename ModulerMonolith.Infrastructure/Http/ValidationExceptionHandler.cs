using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using ModulerMonolith.Core.Results;

namespace ModulerMonolith.Infrastructure.Http;

/// <summary>
/// Query pipeline'ından fırlatılan ValidationException'ı yakalar.
/// Command pipeline'ında bu handler devreye girmez — o tarafta Result.Fail() kullanılır.
/// Yanıt: 422 + { isSuccess, errors, messages } formatında.
/// </summary>
internal sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        if (exception is not ValidationException ve)
            return false;

        var result = Result.Fail(ve.Errors);

        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        await context.Response.WriteAsJsonAsync(result, ct);

        return true;
    }
}
