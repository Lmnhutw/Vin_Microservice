namespace Vin.Services.EmailAPI.Models.Dto
{
    public sealed class CartDTO
    {
        public required CartHeaderDTO CartHeader { get; set; }
        public IEnumerable<CartDetailsDTO>? CartDetails { get; set; }
    }
}
