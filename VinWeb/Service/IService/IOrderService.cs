using Vin.Web.Models;
using Vin.Web.Models.OrderModels;

namespace Vin.Web.Service.IService;

public interface IOrderService
{
    Task<ResponseDTO> CreateOrderAsync(CreateOrderRequestDTO request, CancellationToken cancellationToken = default);
    Task<ResponseDTO> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}
