using AutoMapper;
using Vin.Services.ShoppingCartAPI.Models;

namespace Vin.Services.ShoppingCartAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(Config =>
            {
                Config.CreateMap<CartHeader, CartHeaderDTO>().ReverseMap();
                Config.CreateMap<CartDetails, CartDetailsDTO>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}