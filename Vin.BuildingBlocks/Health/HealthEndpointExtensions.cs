using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Vin.BuildingBlocks.Health;

public static class HealthEndpointExtensions
{
    public static IEndpointRouteBuilder MapVinHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = WriteResponse
        });
        endpoints.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteResponse });
        return endpoints;
    }

    private static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new { status = entry.Value.Status.ToString(), description = entry.Value.Description })
        }));
    }
}
