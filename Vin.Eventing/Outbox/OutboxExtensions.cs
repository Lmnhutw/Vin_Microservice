using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Vin.Contracts;
using Vin.Eventing.Abstractions;

namespace Vin.Eventing.Outbox;

public static class OutboxExtensions
{
    public static void Enqueue<TEvent>(this IEventingDbContext context, TEvent integrationEvent)
        where TEvent : IIntegrationEvent
    {
        var envelope = IntegrationEventEnvelope.From(integrationEvent);
        context.OutboxMessages.Add(new OutboxMessage
        {
            EventId = envelope.EventId,
            EventType = envelope.EventType,
            Version = envelope.Version,
            CorrelationId = envelope.CorrelationId,
            Payload = JsonSerializer.Serialize(envelope, IntegrationEventEnvelope.JsonOptions),
            OccurredOnUtc = envelope.OccurredOnUtc
        });
    }

    public static void ConfigureEventing(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(message => message.EventId);
            entity.Property(message => message.EventType).HasMaxLength(128).IsRequired();
            entity.Property(message => message.CorrelationId).HasMaxLength(128).IsRequired();
            entity.Property(message => message.Payload).IsRequired();
            entity.HasIndex(message => new { message.ProcessedOnUtc, message.DeadLetteredOnUtc, message.OccurredOnUtc });
        });
        modelBuilder.Entity<Inbox.InboxMessage>(entity =>
        {
            entity.HasKey(message => message.EventId);
            entity.Property(message => message.EventType).HasMaxLength(128).IsRequired();
            entity.Property(message => message.CorrelationId).HasMaxLength(128).IsRequired();
        });
    }
}
