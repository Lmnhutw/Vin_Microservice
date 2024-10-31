using Vin.Services.AuthAPI.Models.DTO;

namespace Vin.Services.AuthAPI.Services.IServices
{
    public interface IAuthService
    {
        Task<UserDTO> Register(RegistrationRequestDTO registrationRequestDTO);

        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);

        Task<bool> AssignRole(string email, string rolename);

        Task<bool> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO);

        Task<bool> ResetPassword(ResetPasswordDTO resetPasswordDTO);
    }
}