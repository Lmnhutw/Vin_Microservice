using Microsoft.EntityFrameworkCore;
using Vin.Eventing.Abstractions;
using Vin.Eventing.Inbox;
using Vin.Eventing.Outbox;
using Vin.Services.PaymentAPI.Models;

namespace Vin.Services.PaymentAPI.Data;

public sealed class AppDbContext : DbContext, IEventingDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PaymentTransaction> Payments => Set<PaymentTransaction>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(payment => payment.PaymentTransactionId);
            entity.HasIndex(payment => payment.OrderId).IsUnique();
            entity.Property(payment => payment.Amount).HasPrecision(18, 2);
            entity.Property(payment => payment.PaymentReference).HasMaxLength(64).IsRequired();
            entity.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(payment => payment.RejectionReason).HasMaxLength(512);
        });
        modelBuilder.ConfigureEventing();
    }
}
