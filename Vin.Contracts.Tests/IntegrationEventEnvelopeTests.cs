using System.Text.Json;
using Vin.Contracts;
using Vin.Contracts.Events;
using Xunit;

namespace Vin.Contracts.Tests;

public sealed class IntegrationEventEnvelopeTests
{
    [Fact]
    public void Envelope_round_trip_preserves_version_correlation_and_lines()
    {
        var eventId = Guid.NewGuid();
        var submitted = new OrderSubmittedV1(
            eventId,
            "correlation-123",
            DateTimeOffset.Parse("2026-07-27T00:00:00Z"),
            Guid.NewGuid(),
            "user-42",
            180_000m,
            [new OrderLineV1(7, "Learning product", 90_000m, 2)]);

        var envelope = IntegrationEventEnvelope.From(submitted);
        var restoredEnvelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(
            JsonSerializer.Serialize(envelope, IntegrationEventEnvelope.JsonOptions),
            IntegrationEventEnvelope.JsonOptions)!;
        var restored = restoredEnvelope.Deserialize<OrderSubmittedV1>();

        Assert.Equal(eventId, restoredEnvelope.EventId);
        Assert.Equal(EventTypes.OrderSubmittedV1, restoredEnvelope.EventType);
        Assert.Equal(1, restoredEnvelope.Version);
        Assert.Equal("correlation-123", restoredEnvelope.CorrelationId);
        Assert.Equal(submitted.OrderId, restored.OrderId);
        Assert.Equal(2, restored.Lines.Single().Quantity);
    }

    [Fact]
    public void Payment_authorization_carries_the_order_line_snapshot_for_inventory()
    {
        var authorization = new PaymentAuthorizedV1(
            Guid.NewGuid(), "trace-1", DateTimeOffset.UtcNow, Guid.NewGuid(), "SIM-001",
            [new OrderLineV1(3, "Snapshot", 50m, 1)]);

        var restored = IntegrationEventEnvelope.From(authorization).Deserialize<PaymentAuthorizedV1>();

        Assert.Equal("SIM-001", restored.PaymentReference);
        Assert.Equal(3, restored.Lines.Single().ProductId);
    }
}
