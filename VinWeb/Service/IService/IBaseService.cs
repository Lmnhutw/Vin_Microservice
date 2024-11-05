using Vin.Web.Models;

namespace Vin.Web.Service.IService
{
    public interface IBaseService
    {
        Task<ResponseDTO?> SendAsync(RequestDTO requsetDTO, bool withBearer = true);

    }
}