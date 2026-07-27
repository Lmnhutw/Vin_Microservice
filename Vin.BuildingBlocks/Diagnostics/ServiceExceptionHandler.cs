using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Vin.BuildingBlocks.Diagnostics;

public sealed class ServiceExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ServiceExceptionHandler> _logger;

    public ServiceExceptionHandler(
        IProblemDetailsService problemDetails,
        IHostEnvironment environment,
        ILogger<ServiceExceptionHandler> logger)
    {
        _problemDetails = problemDetails;
        _environment = environment;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        _logger.LogError(exception, "Unhandled request failure. CorrelationId: {CorrelationId}", httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = statusCode;
        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode == StatusCodes.Status500InternalServerError
                    ? "An unexpected service error occurred."
                    : "The request could not be processed.",
                Detail = _environment.IsDevelopment() ? exception.Message : null,
                Type = statusCode == StatusCodes.Status500InternalServerError
                    ? "https://httpstatuses.com/500"
                    : "https://httpstatuses.com/400"
            }
        });
    }
}
