using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Vin.Services.CouponAPI.Models;
using Vin.Services.CouponAPI.Models.DTO;

namespace Vin.Services.CouponAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration InstanceMaps() => new(
            config =>
            {
                config.CreateMap<CouponDTO, Coupon>();
                config.CreateMap<Coupon, CouponDTO>();
            },
            NullLoggerFactory.Instance);
    }
}
