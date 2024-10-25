using Vin.Web.Models;
using Vin.Web.Service.IService;
using Vin.Web.Utility;

namespace Vin.Web.Service
{
    public class CouponService : ICouponService
    {
        private readonly IBaseService _baseService;

        public CouponService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDTO?> CreateCouponsAsync(CouponDTO couponDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.POST,
                Data = couponDTO,
                Url = StaticDetail.CouponAPIBase + "/api/Coupon/AddCoupon"
            });
        }

        public async Task<ResponseDTO?> DeleteCouponAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.DELETE,
                Url = $"{StaticDetail.CouponAPIBase}/api/Coupon/{id}" // Consistent URL
            });
        }

        public async Task<ResponseDTO?> GetAllCouponAsync()
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.GET,
                Url = StaticDetail.CouponAPIBase + "/api/Coupon/GetCouponList"
            });
        }

        public async Task<ResponseDTO?> GetCouponAsync(string couponCode)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.GET,
                Url = $"{StaticDetail.CouponAPIBase}/api/Coupon/GetByCode/{couponCode}"
            });
        }

        public async Task<ResponseDTO?> GetCouponByIdAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.GET,
                Url = $"{StaticDetail.CouponAPIBase}/api/Coupon/GetCouponById/{id}" // Fixed URL
            });
        }

        public async Task<ResponseDTO?> UpdateCouponsAsync(CouponDTO couponDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.PUT,
                Data = couponDTO,
                Url = StaticDetail.CouponAPIBase + "/api/Coupon/UpdateCoupon"
            });
        }
    }
}