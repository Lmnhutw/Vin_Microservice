using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Vin.BuildingBlocks.Diagnostics;

public sealed class RequestTelemetryMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTelemetryMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        using var activity = ServiceTelemetry.ActivitySource.StartActivity(
            $"{context.Request.Method} {context.Request.Path}", ActivityKind.Server);

        try
        {
            await _next(context);
            activity?.SetTag("http.response.status_code", context.Response.StatusCode);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var tags = new TagList
            {
                { "http.request.method", context.Request.Method },
                { "http.route", context.Request.Path.Value ?? "/" },
                { "http.response.status_code", context.Response.StatusCode }
            };
            ServiceTelemetry.RequestCount.Add(1, tags);
            ServiceTelemetry.RequestDuration.Record(stopwatch.Elapsed.TotalMilliseconds, tags);
        }
    }
}
