using Vin.Services.ShoppingCartAPI.Models.DTO;

namespace Vin.Services.ShoppingCartAPI.Service.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetProducts();
    }
}
