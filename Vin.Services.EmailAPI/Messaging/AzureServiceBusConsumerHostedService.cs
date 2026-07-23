using Microsoft.Extensions.Hosting;

namespace Vin.Services.EmailAPI.Messaging
{
    public sealed class AzureServiceBusConsumerHostedService : IHostedService
    {
        private readonly IAzureServiceBusConsumer _consumer;

        public AzureServiceBusConsumerHostedService(IAzureServiceBusConsumer consumer)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        }

        public Task StartAsync(CancellationToken cancellationToken) =>
            _consumer.StartAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) =>
            _consumer.StopAsync(cancellationToken);
    }
}
