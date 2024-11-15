﻿

using Newtonsoft.Json;
using Vin.Services.ShoppingCartAPI.Models.DTO;
using Vin.Services.ShoppingCartAPI.Service.IService;

namespace Vin.Services.ShoppingCartAPI.Service
{
    public class ProductService : IProductService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductService(IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task<IEnumerable<ProductDTO>> GetProducts()
        {

            var client = _httpClientFactory.CreateClient("GetProduct");
            var response = await client.GetAsync("/api/Product/GetProductList");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling Product API: {response.StatusCode}");
            }

            var apiContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(apiContent))
            {
                return new List<ProductDTO>();
            }

            var res = JsonConvert.DeserializeObject<ResponseDTO>(apiContent);
            if (res != null && res.IsSuccess)
            {
                return JsonConvert.DeserializeObject<IEnumerable<ProductDTO>>(Convert.ToString(res.Result));
            }

            return new List<ProductDTO>();
        }

    }
}