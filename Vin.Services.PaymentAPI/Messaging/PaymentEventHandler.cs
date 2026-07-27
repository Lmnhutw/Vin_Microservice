using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vin.Contracts;
using Vin.Contracts.Events;
using Vin.Eventing.Abstractions;
using Vin.Eventing.Outbox;
using Vin.Services.PaymentAPI.Data;
using Vin.Services.PaymentAPI.Models;

namespace Vin.Services.PaymentAPI.Messaging;

public sealed class PaymentEventHandler : IIntegrationEventHandler
{
    private readonly AppDbContext _db;
    private readonly PaymentSimulatorOptions _options;

    public PaymentEventHandler(AppDbContext db, IOptions<PaymentSimulatorOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public bool CanHandle(string eventType, int version) => version == 1 && eventType is
        EventTypes.OrderSubmittedV1 or EventTypes.PaymentRefundRequestedV1;

    public async Task HandleAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken)
    {
        if (envelope.EventType == EventTypes.OrderSubmittedV1)
        {
            await AuthorizeAsync(envelope.Deserialize<OrderSubmittedV1>(), cancellationToken);
            return;
        }
        await RefundAsync(envelope.Deserialize<PaymentRefundRequestedV1>(), envelope.CorrelationId, cancellationToken);
    }

    private async Task AuthorizeAsync(OrderSubmittedV1 order, CancellationToken cancellationToken)
    {
        if (await _db.Payments.AnyAsync(payment => payment.OrderId == order.OrderId, cancellationToken)) return;

        var now = DateTimeOffset.UtcNow;
        var rejected = _options.RejectEveryNthOrder > 0 &&
            order.OrderId.ToByteArray()[0] % _options.RejectEveryNthOrder == 0;
        var payment = new PaymentTransaction
        {
            PaymentTransactionId = Guid.NewGuid(),
            OrderId = order.OrderId,
            Amount = order.TotalAmount,
            PaymentReference = $"SIM-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
            Status = rejected ? PaymentStatus.Rejected : PaymentStatus.Authorized,
            RejectionReason = rejected ? "Deterministic simulator rejection." : null,
            CreatedOnUtc = now,
            UpdatedOnUtc = now
        };
        _db.Payments.Add(payment);
        if (rejected)
        {
            _db.Enqueue(new PaymentRejectedV1(Guid.NewGuid(), order.CorrelationId, now, order.OrderId, payment.RejectionReason!));
            return;
        }
        _db.Enqueue(new PaymentAuthorizedV1(
            Guid.NewGuid(), order.CorrelationId, now, order.OrderId, payment.PaymentReference, order.Lines));
    }

    private async Task RefundAsync(PaymentRefundRequestedV1 request, string correlationId, CancellationToken cancellationToken)
    {
        var payment = await _db.Payments.SingleOrDefaultAsync(item => item.OrderId == request.OrderId, cancellationToken);
        if (payment is null || payment.Status != PaymentStatus.Authorized) return;
        payment.Status = PaymentStatus.Refunded;
        payment.UpdatedOnUtc = DateTimeOffset.UtcNow;
        _db.Enqueue(new PaymentRefundedV1(Guid.NewGuid(), correlationId, DateTimeOffset.UtcNow, request.OrderId, payment.PaymentReference));
    }
}
