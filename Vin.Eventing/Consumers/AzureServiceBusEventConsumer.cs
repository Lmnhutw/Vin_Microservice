using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vin.Contracts;
using Vin.Eventing.Abstractions;
using Vin.Eventing.Inbox;

namespace Vin.Eventing.Consumers;

public sealed class AzureServiceBusEventConsumer<TDbContext> : BackgroundService
    where TDbContext : DbContext, IEventingDbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureServiceBusEventConsumer<TDbContext>> _logger;
    private ServiceBusClient? _client;
    private ServiceBusProcessor? _processor;

    public AzureServiceBusEventConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<AzureServiceBusEventConsumer<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionString = _configuration["ServiceBus:ConnectionString"];
        var topic = _configuration["Messaging:EventsTopic"] ?? "vin-events";
        var subscription = _configuration["Messaging:Subscription"];
        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(subscription))
        {
            _logger.LogWarning("Integration event consumer is disabled: ServiceBus:ConnectionString or Messaging:Subscription is missing.");
            return;
        }

        _client = new ServiceBusClient(connectionString);
        _processor = _client.CreateProcessor(topic, subscription, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
        finally
        {
            await _processor.StopProcessingAsync(CancellationToken.None);
        }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        IntegrationEventEnvelope envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(
                Encoding.UTF8.GetString(args.Message.Body),
                IntegrationEventEnvelope.JsonOptions)
                ?? throw new JsonException("Integration event envelope is empty.");
        }
        catch (JsonException exception)
        {
            await args.DeadLetterMessageAsync(args.Message, "Invalid integration event", exception.Message);
            return;
        }

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = envelope.CorrelationId }))
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var handlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler>()
                .Where(handler => handler.CanHandle(envelope.EventType, envelope.Version))
                .ToArray();
            if (handlers.Length == 0)
            {
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
            if (await db.InboxMessages.AnyAsync(message => message.EventId == envelope.EventId))
            {
                _logger.LogInformation("Duplicate event {EventId} ignored", envelope.EventId);
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            await using var transaction = await db.Database.BeginTransactionAsync(args.CancellationToken);
            db.InboxMessages.Add(new InboxMessage
            {
                EventId = envelope.EventId,
                EventType = envelope.EventType,
                CorrelationId = envelope.CorrelationId,
                ReceivedOnUtc = DateTimeOffset.UtcNow
            });
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(envelope, args.CancellationToken);
            }
            await db.SaveChangesAsync(args.CancellationToken);
            await transaction.CommitAsync(args.CancellationToken);
            await args.CompleteMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Integration event processor error. Entity={EntityPath}, Source={ErrorSource}", args.EntityPath, args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        if (_processor is not null)
        {
            _processor.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        if (_client is not null)
        {
            _client.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        base.Dispose();
    }
}
