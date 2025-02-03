using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vin.MessageBus.Settings;

namespace Vin.MessageBus
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMessageBusService(this IServiceCollection services, Action<IConfigurationBuilder> configure /*configuration*/)
        {
            // Build a new configuration for the library
            var configurationBuilder = new ConfigurationBuilder();
            configure(configurationBuilder); // Apply custom configuration
            var configuration = configurationBuilder.Build();

            // Bind ServiceBusSettings from the custom configuration
            var serviceBusSettings = new ServiceBusSettings();
            configuration.GetSection("ServiceBus").Bind(serviceBusSettings);

            // Register settings for DI
            services.AddSingleton(serviceBusSettings);

            // Register Service Bus client
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusClient>>();
                logger.LogInformation("Initializing ServiceBusClient...");
                return new ServiceBusClient(serviceBusSettings.MsB_ConnectionString);
            });

            return services;
        }
    }
}
