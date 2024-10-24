using Microsoft.EntityFrameworkCore;
using Vin.Services.CouponAPI.Models;

namespace Vin.Services.CouponAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Coupon> Coupons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Coupon>().HasData(new Coupon
            {
                CouponId = 1,
                CouponCode = "OFF10",
                DiscountAmount = 10,
                MinAmount = 20,
            }); modelBuilder.Entity<Coupon>().HasData(new Coupon
            {
                CouponId = 2,
                CouponCode = "OFF20",
                DiscountAmount = 20,
                MinAmount = 40,
            });
        }
    }
}