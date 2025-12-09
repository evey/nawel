using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Nawel.Api.Exceptions;

namespace Nawel.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString();

        _logger.LogError(
            exception,
            "Exception occurred: {Message} | CorrelationId: {CorrelationId}",
            exception.Message,
            correlationId);

        var (statusCode, message) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message),
            BadRequestException badRequest => (HttpStatusCode.BadRequest, badRequest.Message),
            UnauthorizedException unauthorized => (HttpStatusCode.Unauthorized, unauthorized.Message),
            InvalidOperationException invalidOp => (HttpStatusCode.BadRequest, invalidOp.Message),
            _ => (HttpStatusCode.InternalServerError, "An internal server error occurred")
        };

        var response = new
        {
            message = message,
            correlationId = correlationId,
            timestamp = DateTime.UtcNow
        };

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(response),
            cancellationToken);

        return true;
    }
}
