namespace Vin.Web.Models.OrderModels;

public sealed class CreateOrderRequestDTO
{
    public IReadOnlyCollection<CreateOrderLineRequestDTO> Lines { get; init; } = Array.Empty<CreateOrderLineRequestDTO>();
}

public sealed class CreateOrderLineRequestDTO
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}
