using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vin.Contracts;
using Vin.Eventing.Abstractions;
using Vin.MessageBus;

namespace Vin.Eventing.Outbox;

public sealed class OutboxPublisher<TDbContext> : BackgroundService
    where TDbContext : DbContext, IEventingDbContext
{
    private const int DeadLetterAttemptLimit = 10;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OutboxPublisher<TDbContext>> _logger;

    public OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<OutboxPublisher<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishBatchAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task PublishBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var topic = _configuration["Messaging:EventsTopic"] ?? "vin-events";
        var pending = await db.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null && message.DeadLetteredOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(
                    message.Payload,
                    IntegrationEventEnvelope.JsonOptions)
                    ?? throw new JsonException("Outbox payload is empty.");

                await bus.PublishMessageAsync(
                    envelope,
                    topic,
                    new MessageBusMetadata(envelope.CorrelationId, envelope.EventId.ToString("N"), envelope.EventType, envelope.Version),
                    cancellationToken);

                message.ProcessedOnUtc = DateTimeOffset.UtcNow;
                message.LastError = null;
            }
            catch (Exception exception)
            {
                message.Attempts++;
                message.LastError = exception.Message;
                if (message.Attempts >= DeadLetterAttemptLimit)
                {
                    message.DeadLetteredOnUtc = DateTimeOffset.UtcNow;
                    _logger.LogError(exception, "Outbox event {EventId} reached the local retry limit and was marked dead-lettered", message.EventId);
                }
                else
                {
                    _logger.LogWarning(exception, "Outbox publish failed for {EventId}; attempt {Attempt}", message.EventId, message.Attempts);
                }
            }
        }

        if (pending.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
