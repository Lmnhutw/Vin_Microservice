using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Vin.BuildingBlocks.Health;

public static class HealthCheckBuilderExtensions
{
    public static IHealthChecksBuilder AddDbContextReadinessCheck<TContext>(
        this IHealthChecksBuilder builder,
        string name = "database") where TContext : DbContext
        => builder.AddCheck<DbContextReadinessHealthCheck<TContext>>(name, tags: ["ready"]);
}
