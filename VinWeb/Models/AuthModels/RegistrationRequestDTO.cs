namespace Vin.Web.Models.AuthModels
{
    public class RegistrationRequestDTO
    {
        required
        public string Email
        { get; set; }
        required
        public string FullName
        { get; set; }
        required
        public string PhoneNumber
        { get; set; }
        required
        public string Password
        { get; set; }
        public string? Role { get; set; }
    }
}