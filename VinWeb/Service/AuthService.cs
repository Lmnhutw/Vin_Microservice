using Vin.Web.Models;
using Vin.Web.Models.AuthModels;
using Vin.Web.Service.IService;
using Vin.Web.Utility;

namespace Vin.Web.Service
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService _baseService;

        public AuthService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDTO> AssignRoleAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.POST,
                Data = registrationRequestDTO,
                Url = StaticDetail.AuthAPIBase + "/api/auth/AssignRole"
            });
        }

        public async Task<ResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.POST,
                Data = loginRequestDTO,
                Url = StaticDetail.AuthAPIBase + "/api/auth/AssignRole"
            });
        }

        public async Task<ResponseDTO> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.POST,
                Data = registrationRequestDTO,
                Url = StaticDetail.AuthAPIBase + "/api/auth/Register"
            });
        }


        public async Task<ResponseDTO> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDTO)
        {
            return await _baseService.SendAsync(new RequestDTO
            {
                ApiType = StaticDetail.ApiType.POST,
                Data = forgotPasswordDTO,
                Url = StaticDetail.AuthAPIBase + "/api/auth/ForgotPassword"
            });
        }

        public async Task<ResponseDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {
            return await _baseService.SendAsync(new RequestDTO
            {
                ApiType = StaticDetail.ApiType.POST,
                Data = resetPasswordDTO,
                Url = StaticDetail.AuthAPIBase + "/api/auth/ResetPassword"
            });
        }


    }
}