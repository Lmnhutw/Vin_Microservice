using Newtonsoft.Json;
using Vin.Services.ShoppingCartAPI.Models.DTO;

namespace Vin.Services.ShoppingCartAPI.Service.IService
{
    public class CouponService : ICouponService
    {


        private readonly IHttpClientFactory _httpClientFactory;

        public CouponService(IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;

        }

        public async Task<CouponDTO> GetCoupon(string couponCode)
        {
            var client = _httpClientFactory.CreateClient("GetCoupon");
            var response = await client.GetAsync($"/api/Coupon/GetByCode/{couponCode}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling Coupon API: {response.StatusCode}");
            }

            var apiContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(apiContent))
            {
                return new CouponDTO();
            }

            var res = JsonConvert.DeserializeObject<ResponseDTO>(apiContent);
            if (res?.IsSuccess == true && res.Result != null)
            {
                return JsonConvert.DeserializeObject<CouponDTO>(Convert.ToString(res.Result));
            }

            return new CouponDTO();
        }


    }
}
