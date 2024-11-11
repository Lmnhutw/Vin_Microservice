using AutoMapper;
using Vin.Services.ShoppingCartAPI.Models;
using Vin.Services.ShoppingCartAPI.Models.DTO;

namespace Vin.Services.ShoppingCartAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<CartHeader, CartHeaderDTO>().ReverseMap();
                config.CreateMap<CartDetails, CartDetailsDTO>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}