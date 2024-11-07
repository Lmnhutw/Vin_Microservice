using AutoMapper;
using Vin.Services.ProductAPI.Models;
using Vin.Services.ProductAPI.Models.DTO;

namespace Vin.Services.ProductAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration InstanceMaps()
        {
            var mappingConfig = new MapperConfiguration(Config =>
            {
                Config.CreateMap<ProductDTO, Product>().ReverseMap();
                //Config.CreateMap <ProductDTO, Product > (); -> the .ReverseMap helps automaticly create this line.
            });
            return mappingConfig;
        }
    }
}