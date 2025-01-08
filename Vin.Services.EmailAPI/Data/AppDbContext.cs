using Microsoft.EntityFrameworkCore;
using Vin.Services.EmailAPI.Models;


namespace Vin.Services.EmailAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<EmailLogger> EmailLogger { get; set; }

    }
}