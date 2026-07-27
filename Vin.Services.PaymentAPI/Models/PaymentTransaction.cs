namespace Vin.Services.PaymentAPI.Models;

public sealed class PaymentTransaction
{
    public Guid PaymentTransactionId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }
}
