using Vin.Web.Models.AuthModels;

namespace Vin.Web.Service.IService
{
    public interface IAuthService
    {
        Task<ResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO);

        Task<ResponseDTO> RegisterAsync(RegistrationRequestDTO registrationRequestDTO);

        Task<ResponseDTO> AssignRoleAsync(RegistrationRequestDTO registrationRequestDTO);

        Task<ResponseDTO> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDTO);

        Task<ResponseDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);
    }
}