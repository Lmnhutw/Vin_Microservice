using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Vin.Services.ShoppingCartAPI.Models;
using Vin.Services.ShoppingCartAPI.Models.DTO;

namespace Vin.Services.ShoppingCartAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration RegisterMaps() => new(
            config =>
            {
                config.CreateMap<CartHeader, CartHeaderDTO>().ReverseMap();
                config.CreateMap<CartDetails, CartDetailsDTO>().ReverseMap();
            },
            NullLoggerFactory.Instance);
    }
}
