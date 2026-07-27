using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vin.Eventing.Abstractions;
using Vin.Eventing.Consumers;
using Vin.Eventing.Outbox;

namespace Vin.Eventing;

public static class EventingServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxPublisher<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext, IEventingDbContext
    {
        services.AddHostedService<OutboxPublisher<TDbContext>>();
        return services;
    }

    public static IServiceCollection AddIntegrationEventConsumer<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext, IEventingDbContext
    {
        services.AddHostedService<AzureServiceBusEventConsumer<TDbContext>>();
        return services;
    }
}
