using Vin.Web.Models;
using Vin.Web.Service.IService;
using Vin.Web.Utility;

namespace Vin.Web.Service
{
    public class ProductService : IProductService
    {
        private readonly IBaseService _baseService;

        public ProductService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDTO?> CreateProductsAsync(ProductDTO couponDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.POST,
                Data = couponDTO,
                Url = StaticDetail.ProductAPIBase + "/api/Product/AddProduct"
            });
        }

        public async Task<ResponseDTO?> DeleteProductAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.DELETE,
                Url = $"{StaticDetail.ProductAPIBase}/api/Product/{id}"
                // Consistent URL
            });
        }

        public async Task<ResponseDTO?> GetAllProductAsync()
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.GET,
                Url = StaticDetail.ProductAPIBase + "/api/Product/GetProductList"
            });
        }
        //Get product by name, fixing later
        /*public async Task<ResponseDTO?> GetProductAsync(string couponCode)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.GET,
                Url = $"{StaticDetail.ProductAPIBase}/api/Product/GetByCode/{couponCode}"
            });
        }*/

        public async Task<ResponseDTO?> GetProductByIdAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.GET,
                Url = $"{StaticDetail.ProductAPIBase}/api/Product/GetProductById/{id}" // Fixed URL
            });
        }

        public async Task<ResponseDTO?> UpdateProductsAsync(ProductDTO couponDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.PUT,
                Data = couponDTO,
                Url = StaticDetail.ProductAPIBase + "/api/Product/UpdateProduct"
            });
        }
    }
}