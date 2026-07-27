using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Vin.BuildingBlocks.Health;

public sealed class DbContextReadinessHealthCheck<TContext> : IHealthCheck where TContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DbContextReadinessHealthCheck(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<TContext>().Database;
        var canConnect = await database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("Database connection succeeded.")
            : HealthCheckResult.Unhealthy("Database connection failed.");
    }
}
