using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Vin.MessageBus.Settings;

namespace Vin.MessageBus
{
    public class MessageBus : IMessageBus, IAsyncDisposable
    {
        private readonly ServiceBusSettings _serviceBusSettings;
        private readonly ILogger<MessageBus> _logger;
        private readonly ServiceBusClient _client;

        public MessageBus(ServiceBusSettings serviceBusSettings, ILogger<MessageBus> logger)
        {
            _serviceBusSettings = serviceBusSettings ??
                throw new ArgumentNullException(nameof(serviceBusSettings));
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_serviceBusSettings.MsB_ConnectionString))
            {
                throw new InvalidOperationException("Service Bus connection string is not configured.");
            }

            try
            {
                _client = new ServiceBusClient(_serviceBusSettings.MsB_ConnectionString);
                _logger.LogInformation("ServiceBusClient initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize ServiceBusClient");
                throw;
            }
        }

        public async Task PublishMessageAsync(object message, string topic_queue_Name)
        {
            if (_client == null)
            {
                throw new InvalidOperationException("ServiceBusClient is not initialized");
            }

            await using var sender = _client.CreateSender(topic_queue_Name);

            try
            {
                var messageBody = JsonSerializer.Serialize(message);
                var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    ContentType = "application/json"
                };

                _logger.LogInformation("Sending message to {topic_queue_Name}", topic_queue_Name);
                await sender.SendMessageAsync(serviceBusMessage);
                _logger.LogInformation("Message sent successfully to {topic_queue_Name}", topic_queue_Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to {topic_queue_Name}", topic_queue_Name);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _client.DisposeAsync();
        }
    }
}
