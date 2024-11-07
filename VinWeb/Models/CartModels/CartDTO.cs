namespace Vin.Web.Models.CartModels
{
    public class CartDTO
    {
        public CartHeaderDTO CartHeader { get; set; }
        public IEnumerable<CartDetailsDTO> CartDetails { get; set; } //this help app has 1 or multiple cart 
    }
}
