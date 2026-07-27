using Microsoft.EntityFrameworkCore;
using Vin.Contracts;
using Vin.Contracts.Events;
using Vin.Eventing.Abstractions;
using Vin.Eventing.Outbox;
using Vin.Services.InventoryAPI.Data;
using Vin.Services.InventoryAPI.Models;

namespace Vin.Services.InventoryAPI.Messaging;

public sealed class InventoryEventHandler : IIntegrationEventHandler
{
    private readonly AppDbContext _db;

    public InventoryEventHandler(AppDbContext db) => _db = db;

    public bool CanHandle(string eventType, int version)
        => version == 1 && eventType == EventTypes.PaymentAuthorizedV1;

    public async Task HandleAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payment = envelope.Deserialize<PaymentAuthorizedV1>();
        if (await _db.Reservations.AnyAsync(reservation => reservation.OrderId == payment.OrderId, cancellationToken)) return;

        var productIds = payment.Lines.Select(line => line.ProductId).Distinct().ToArray();
        var stock = await _db.InventoryItems.Where(item => productIds.Contains(item.ProductId))
            .ToDictionaryAsync(item => item.ProductId, cancellationToken);
        var shortage = payment.Lines.FirstOrDefault(line =>
            !stock.TryGetValue(line.ProductId, out var item) || item.AvailableQuantity < line.Quantity);
        var now = DateTimeOffset.UtcNow;
        if (shortage is not null)
        {
            var reason = $"Insufficient configured inventory for product {shortage.ProductId}.";
            _db.Reservations.Add(new InventoryReservation
            {
                InventoryReservationId = Guid.NewGuid(),
                OrderId = payment.OrderId,
                Status = InventoryReservationStatus.Rejected,
                FailureReason = reason,
                CreatedOnUtc = now
            });
            _db.Enqueue(new InventoryRejectedV1(Guid.NewGuid(), envelope.CorrelationId, now, payment.OrderId, reason));
            return;
        }

        var reservation = new InventoryReservation
        {
            InventoryReservationId = Guid.NewGuid(),
            OrderId = payment.OrderId,
            Status = InventoryReservationStatus.Reserved,
            CreatedOnUtc = now,
            Lines = payment.Lines.Select(line => new InventoryReservationLine
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity
            }).ToArray()
        };
        foreach (var line in payment.Lines)
        {
            var item = stock[line.ProductId];
            item.AvailableQuantity -= line.Quantity;
            item.UpdatedOnUtc = now;
        }
        _db.Reservations.Add(reservation);
        _db.Enqueue(new InventoryReservedV1(Guid.NewGuid(), envelope.CorrelationId, now, payment.OrderId, payment.Lines));
    }
}
