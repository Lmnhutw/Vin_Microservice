using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Vin.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private readonly string _serviceBus;
        private readonly ILogger<MessageBus> _logger;
        private readonly IConfiguration configuration;
        private readonly ServiceBusClient _client;


        /* public MessageBus(IConfiguration _configuration, ILogger<MessageBus> logger)
         {
             _logger = logger;
             _configuration = configuration;
             _serviceBus = configuration.GetValue<string>("ServiceBusConnection");


             if (string.IsNullOrWhiteSpace(_serviceBus))
             {
                 _logger.LogError("ServiceBusConnection is not configured.");
                 throw new InvalidOperationException("ServiceBusConnection is not configured.");
             }

             _logger.LogInformation("ServiceBusConnection is configured successfully.");
         }*/

        public async Task PublishMessageAsync<T>(T message, string queueOrTopicName)
        {
            var sender = _client.CreateSender(queueOrTopicName);

            try
            {
                var messageBody = JsonSerializer.Serialize(message);
                var serviceBusMessage = new ServiceBusMessage(messageBody);

                _logger.LogInformation("Sending message to {QueueOrTopic}", queueOrTopicName);
                await sender.SendMessageAsync(serviceBusMessage);
                _logger.LogInformation("Message sent successfully to {QueueOrTopic}", queueOrTopicName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to {QueueOrTopic}", queueOrTopicName);
                throw;
            }
            finally
            {
                await sender.DisposeAsync();
            }
        }


        /* public async Task PublishMessage(object message, string topic_queue_Name)
         {
             try
             {
                 await using var client = new ServiceBusClient(_serviceBus);
                 ServiceBusSender sender = client.CreateSender(topic_queue_Name);

                 var jsonMessage = JsonConvert.SerializeObject(message);
                 ServiceBusMessage finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
                 {
                     CorrelationId = Guid.NewGuid().ToString(),
                 };

                 await sender.SendMessageAsync(finalMessage);
                 _logger.LogInformation("Message sent to {TopicQueueName} successfully.", topic_queue_Name);
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error sending message to {TopicQueueName}.", topic_queue_Name);
                 throw;
             }
         }*/
    }
}




