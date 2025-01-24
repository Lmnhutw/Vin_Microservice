
using Vin.Services.EmailAPI.Models.Dto;

namespace Vin.Services.EmailAPI.Services
{
    public interface IEmailService
    {
        Task EmailCartAndLog(CartDTO cartDto);
        //Task RegisterUserEmailAndLog(string email);
        //Task LogOrderPlaced(RewardsMessage rewardsDto);
    }
}
