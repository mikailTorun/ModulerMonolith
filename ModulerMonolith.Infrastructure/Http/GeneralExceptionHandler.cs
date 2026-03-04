using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModulerMonolith.Core.Results;

namespace ModulerMonolith.Infrastructure.Http;

/// <summary>
/// Yakalanmamış tüm exception'ları karşılar.
/// Yanıt: 500 + { isSuccess: false, errors: [{property: "", message: "Beklenmedik bir hata oluştu."}], messages: [] }
/// Detaylı hata bilgisi loglara yazılır, client'a asla sızdırılmaz.
/// </summary>
internal sealed class GeneralExceptionHandler(ILogger<GeneralExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        logger.LogError(exception, "Unhandled exception on {Method} {Path}",
            context.Request.Method, context.Request.Path);

        var result = Result.Fail("Beklenmedik bir hata oluştu.");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(result, ct);

        return true;
    }
}
