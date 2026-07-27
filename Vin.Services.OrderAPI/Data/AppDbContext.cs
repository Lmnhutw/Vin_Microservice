using Microsoft.EntityFrameworkCore;
using Vin.Eventing.Abstractions;
using Vin.Eventing.Inbox;
using Vin.Eventing.Outbox;
using Vin.Services.OrderAPI.Models;

namespace Vin.Services.OrderAPI.Data;

public sealed class AppDbContext : DbContext, IEventingDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(order => order.OrderId);
            entity.Property(order => order.OrderNumber).HasMaxLength(32).IsRequired();
            entity.HasIndex(order => order.OrderNumber).IsUnique();
            entity.Property(order => order.UserId).HasMaxLength(128).IsRequired();
            entity.Property(order => order.TotalAmount).HasPrecision(18, 2);
            entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(order => order.FailureReason).HasMaxLength(512);
            entity.HasIndex(order => new { order.UserId, order.CreatedOnUtc });
            entity.HasMany(order => order.Lines).WithOne(line => line.Order).HasForeignKey(line => line.OrderId);
        });
        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(line => line.OrderLineId);
            entity.Property(line => line.ProductName).HasMaxLength(256).IsRequired();
            entity.Property(line => line.UnitPrice).HasPrecision(18, 2);
            entity.HasIndex(line => new { line.OrderId, line.ProductId });
        });
        modelBuilder.ConfigureEventing();
    }
}
