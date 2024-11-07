using Vin.Web.Models;

namespace Vin.Web.Service.IService
{
    public interface IProductService
    {
        //Task<ResponseDTO?> GetProductAsync(string productCode); changing into Name

        Task<ResponseDTO?> GetAllProductAsync();

        Task<ResponseDTO?> GetProductByIdAsync(int id);

        Task<ResponseDTO?> CreateProductsAsync(ProductDTO productDTO);

        Task<ResponseDTO?> UpdateProductsAsync(ProductDTO productDTO);

        Task<ResponseDTO?> DeleteProductAsync(int id);
    }
}