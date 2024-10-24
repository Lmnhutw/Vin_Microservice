using AutoMapper;
using Vin.Services.CouponAPI.Models;
using Vin.Services.CouponAPI.Models.DTO;

namespace Vin.Services.CouponAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration InstanceMaps()
        {
            var mappingConfig = new MapperConfiguration(Config =>
            {
                Config.CreateMap<CouponDTO, Coupon>();
                Config.CreateMap<Coupon, CouponDTO>();
            });
            return mappingConfig;
        }
    }
}