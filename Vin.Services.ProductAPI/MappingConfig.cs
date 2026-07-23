using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Vin.Services.ProductAPI.Models;
using Vin.Services.ProductAPI.Models.DTO;

namespace Vin.Services.ProductAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration InstanceMaps() => new(
            config => config.CreateMap<ProductDTO, Product>().ReverseMap(),
            NullLoggerFactory.Instance);
    }
}
