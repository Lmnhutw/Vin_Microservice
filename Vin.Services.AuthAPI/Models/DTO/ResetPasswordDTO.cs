namespace Vin.Services.AuthAPI.Models.DTO
{
    public class ResetPasswordDTO
    {
        required
        public string Email
        { get; set; }
        required
        public string Token
        { get; set; }
        required
        public string NewPassword
        { get; set; }


    }
}
