using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Vin.Services.EmailAPI.Models.Dto;
using Vin.Services.EmailAPI.Services;

namespace Vin.Services.EmailAPI.Messaging
{
    public sealed class AzureServiceBusConsumer : IAzureServiceBusConsumer, IAsyncDisposable
    {
        private readonly string _emailCartQueue;
        private readonly ILogger<AzureServiceBusConsumer> _logger;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _emailCartProcessor;
        private readonly IEmailService _emailService;

        public AzureServiceBusConsumer(
            IConfiguration configuration,
            ILogger<AzureServiceBusConsumer> logger,
            IEmailService emailService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));

            _emailCartQueue = configuration["TopicAndQueueNames:EmailShoppingCartQueue"]
                ?? throw new InvalidOperationException(
                    "Configuration is missing TopicAndQueueNames:EmailShoppingCartQueue.");

            var connectionString = configuration["ServiceBus:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Configuration is missing ServiceBus:ConnectionString.");

            _client = new ServiceBusClient(connectionString);
            _emailCartProcessor = _client.CreateProcessor(
                _emailCartQueue,
                new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = 1,
                    AutoCompleteMessages = false
                });
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailCartProcessor.StartProcessingAsync(cancellationToken);

            _logger.LogInformation(
                "Started Azure Service Bus consumer for queue {QueueName}",
                _emailCartQueue);
        }

        private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            _logger.LogInformation(
                "Received email-cart message with correlation {CorrelationId}",
                message.CorrelationId);

            try
            {
                var body = Encoding.UTF8.GetString(message.Body);
                var cartDto = JsonSerializer.Deserialize<CartDTO>(body);

                if (cartDto?.CartHeader is null || string.IsNullOrWhiteSpace(cartDto.CartHeader.Email))
                {
                    await args.DeadLetterMessageAsync(
                        message,
                        "Invalid cart message",
                        "CartHeader and CartHeader.Email are required.");
                    return;
                }

                await _emailService.SendEmailAsync(cartDto);
                await args.CompleteMessageAsync(message);

                _logger.LogInformation(
                    "Completed email-cart message {CorrelationId}",
                    message.CorrelationId);
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "Email-cart message payload is invalid");
                await args.DeadLetterMessageAsync(
                    message,
                    "Deserialization failed",
                    exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Email-cart message processing failed; message will be retried");
                throw;
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(
                args.Exception,
                "Azure Service Bus processor error. Entity={EntityPath}, Source={ErrorSource}",
                args.EntityPath,
                args.ErrorSource);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _emailCartProcessor.StopProcessingAsync(cancellationToken);
            _logger.LogInformation("Stopped Azure Service Bus consumer for queue {QueueName}", _emailCartQueue);
        }

        public async ValueTask DisposeAsync()
        {
            await _emailCartProcessor.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
