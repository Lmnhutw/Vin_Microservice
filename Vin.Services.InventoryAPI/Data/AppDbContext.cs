using Microsoft.EntityFrameworkCore;
using Vin.Eventing.Abstractions;
using Vin.Eventing.Inbox;
using Vin.Eventing.Outbox;
using Vin.Services.InventoryAPI.Models;

namespace Vin.Services.InventoryAPI.Data;

public sealed class AppDbContext : DbContext, IEventingDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryReservation> Reservations => Set<InventoryReservation>();
    public DbSet<InventoryReservationLine> ReservationLines => Set<InventoryReservationLine>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(item => item.ProductId);
            entity.ToTable(table => table.HasCheckConstraint("CK_InventoryItems_AvailableQuantity", "AvailableQuantity >= 0"));
        });
        modelBuilder.Entity<InventoryReservation>(entity =>
        {
            entity.HasKey(reservation => reservation.InventoryReservationId);
            entity.HasIndex(reservation => reservation.OrderId).IsUnique();
            entity.Property(reservation => reservation.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(reservation => reservation.FailureReason).HasMaxLength(512);
            entity.HasMany(reservation => reservation.Lines).WithOne(line => line.Reservation)
                .HasForeignKey(line => line.InventoryReservationId);
        });
        modelBuilder.Entity<InventoryReservationLine>(entity =>
        {
            entity.HasKey(line => line.InventoryReservationLineId);
            entity.HasIndex(line => new { line.InventoryReservationId, line.ProductId });
        });
        modelBuilder.ConfigureEventing();
    }
}
