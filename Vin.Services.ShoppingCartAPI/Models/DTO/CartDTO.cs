namespace Vin.Services.ShoppingCartAPI.Models.DTO
{
    public class CartDTO
    {
        public CartHeaderDTO CartHeader { get; set; }
        public IEnumerable<CartDetailsDTO> CartDetails { get; set; } //this help app has 1 or multiple cart 
    }
}
