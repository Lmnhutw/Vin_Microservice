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
            // Create a configuration builder and inherit existing configuration
            var configBuilder = new ConfigurationBuilder();

            // Apply the custom configuration sources provided by the main app
            configureConfiguration(configBuilder);

            // Build the configuration (merged)
            var configuration = configBuilder.Build();

            // Read Service Bus Connection String
            var serviceBusSettings = new ServiceBusSettings
            {
                MsB_ConnectionString = configuration["ServiceBus:ConnectionString"]
                    ?? configuration.GetConnectionString("ServiceBusConnectionString")
            };

            // Validate configuration
            if (string.IsNullOrWhiteSpace(serviceBusSettings.MsB_ConnectionString))
            {
                throw new InvalidOperationException(
                    "Service Bus connection string is not configured. " +
                    "Please set it in configuration under 'ServiceBus:ConnectionString' or in ConnectionStrings section.");
            }

            // Register settings in DI
            services.AddSingleton(serviceBusSettings);
            services.AddTransient<IMessageBus, MessageBus>();

            return services;
        }
    }
}
