using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Vin.MessageBus.Settings;

namespace Vin.MessageBus
{
    public sealed class MessageBus : IMessageBus, IAsyncDisposable
    {
        private readonly ServiceBusSettings _serviceBusSettings;
        private readonly ILogger<MessageBus> _logger;
        private readonly ServiceBusClient? _client;

        public MessageBus(ServiceBusSettings serviceBusSettings, ILogger<MessageBus> logger)
        {
            _serviceBusSettings = serviceBusSettings ?? throw new ArgumentNullException(nameof(serviceBusSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (_serviceBusSettings.IsConfigured)
            {
                _client = new ServiceBusClient(_serviceBusSettings.MsB_ConnectionString!);
                _logger.LogInformation("Azure Service Bus client initialized");
            }
            else
            {
                _logger.LogWarning(
                    "Azure Service Bus is not configured. Message publishing is disabled until ServiceBus:ConnectionString is provided.");
            }
        }

        public async Task PublishMessageAsync(
            object message,
            string topicQueueName,
            CancellationToken cancellationToken = default)
            => await PublishMessageAsync(
                message,
                topicQueueName,
                new MessageBusMetadata(Guid.NewGuid().ToString("N")),
                cancellationToken);

        public async Task PublishMessageAsync(
            object message,
            string topicQueueName,
            MessageBusMetadata metadata,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(metadata);
            if (string.IsNullOrWhiteSpace(topicQueueName))
            {
                throw new ArgumentException("A Service Bus queue or topic name is required.", nameof(topicQueueName));
            }

            if (_client is null)
            {
                throw new InvalidOperationException(
                    "Azure Service Bus is not configured. Set ServiceBus:ConnectionString before publishing messages.");
            }

            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
            {
                MessageId = metadata.MessageId ?? Guid.NewGuid().ToString("N"),
                CorrelationId = metadata.CorrelationId,
                ContentType = "application/json"
            };
            if (!string.IsNullOrWhiteSpace(metadata.EventType))
            {
                serviceBusMessage.ApplicationProperties["eventType"] = metadata.EventType;
            }
            if (metadata.EventVersion is not null)
            {
                serviceBusMessage.ApplicationProperties["eventVersion"] = metadata.EventVersion.Value;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                await using var sender = _client.CreateSender(topicQueueName);
                await sender.SendMessageAsync(serviceBusMessage, cancellationToken);

                _logger.LogInformation(
                    "Published Service Bus message to {QueueName} with correlation {CorrelationId} in {ElapsedMs} ms",
                    topicQueueName,
                    serviceBusMessage.CorrelationId,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (ServiceBusException exception)
            {
                _logger.LogError(
                    exception,
                    "Service Bus publish failed for {QueueName}. Reason={Reason}, IsTransient={IsTransient}",
                    topicQueueName,
                    exception.Reason,
                    exception.IsTransient);
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Message publish failed for {QueueName}", topicQueueName);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_client is not null)
            {
                await _client.DisposeAsync();
            }
        }
    }
}
