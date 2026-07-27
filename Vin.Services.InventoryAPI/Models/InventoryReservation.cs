namespace Vin.Services.InventoryAPI.Models;

public enum InventoryReservationStatus
{
    Reserved,
    Rejected
}

public sealed class InventoryReservation
{
    public Guid InventoryReservationId { get; set; }
    public Guid OrderId { get; set; }
    public InventoryReservationStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public ICollection<InventoryReservationLine> Lines { get; set; } = new List<InventoryReservationLine>();
}

public sealed class InventoryReservationLine
{
    public long InventoryReservationLineId { get; set; }
    public Guid InventoryReservationId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public InventoryReservation? Reservation { get; set; }
}
