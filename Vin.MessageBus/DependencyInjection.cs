using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vin.MessageBus.Settings;

namespace Vin.MessageBus
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMessageBusService(
            this IServiceCollection services,
            Action<IConfigurationBuilder> configureConfiguration)
        {
            ArgumentNullException.ThrowIfNull(configureConfiguration);

            var configBuilder = new ConfigurationBuilder();
            configureConfiguration(configBuilder);
            var configuration = configBuilder.Build();

            var serviceBusSettings = new ServiceBusSettings
            {
                MsB_ConnectionString = configuration["ServiceBus:ConnectionString"]
            };

            // Service Bus is optional for local development. MessageBus reports a clear
            // configuration error only when a caller attempts to publish without it.
            services.AddSingleton(serviceBusSettings);
            services.AddSingleton<IMessageBus, MessageBus>();

            return services;
        }
    }
}
