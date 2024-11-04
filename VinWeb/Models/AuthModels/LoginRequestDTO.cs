using System.ComponentModel.DataAnnotations;

namespace Vin.Web.Models.AuthModels
{
    public class LoginRequestDTO
    {
        [Required]
        public string UserName
        { get; set; }
        [Required]
        public string Password
        { get; set; }
    }
}