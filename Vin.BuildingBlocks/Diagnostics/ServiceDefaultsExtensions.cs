using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Vin.BuildingBlocks.Diagnostics;

public static class ServiceDefaultsExtensions
{
    public static IServiceCollection AddVinServiceDefaults(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
                context.ProblemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.TraceId.ToString()
                    ?? context.HttpContext.TraceIdentifier;
            };
        });
        services.AddExceptionHandler<ServiceExceptionHandler>();
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationPropagationHandler>();
        services.AddTransient<ResilientHttpMessageHandler>();

        return services;
    }

    public static IApplicationBuilder UseVinServiceDefaults(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseExceptionHandler();
        app.UseMiddleware<RequestTelemetryMiddleware>();
        return app;
    }

    public static IHttpClientBuilder AddVinResilience(this IHttpClientBuilder builder)
    {
        builder.ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(8));
        builder.AddHttpMessageHandler<CorrelationPropagationHandler>();
        builder.AddHttpMessageHandler<ResilientHttpMessageHandler>();
        return builder;
    }
}
