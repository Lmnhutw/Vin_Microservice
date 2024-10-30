using Microsoft.AspNetCore.Identity;

namespace Vin.Services.AuthAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}