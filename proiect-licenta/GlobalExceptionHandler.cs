using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using proiect_licenta.Exceptions;

namespace proiect_licenta;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, error) = exception switch
        {
            UnauthorizedException => (401, exception.Message),
            ForbiddenException => (403, exception.Message),
            NotFoundException => (404, exception.Message),
            _ => (500, exception.Message)
        };
            
        httpContext.Response.StatusCode = statusCode;
        var json = JsonSerializer.Serialize(new { error, statusCode });
        await httpContext.Response.WriteAsync(json, cancellationToken);
        return true;
    }
}