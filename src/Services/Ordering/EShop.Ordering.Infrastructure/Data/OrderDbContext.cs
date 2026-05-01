using EShop.Ordering.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Ordering.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderName).IsRequired();
            entity.Property(e => e.CustomerId).IsRequired();

            entity.OwnsOne(e => e.ShippingAddress, address =>
            {
                address.Property(a => a.FirstName).IsRequired();
                address.Property(a => a.LastName).IsRequired();
                address.Property(a => a.EmailAddress);
                address.Property(a => a.AddressLine);
                address.Property(a => a.Country);
                address.Property(a => a.State);
                address.Property(a => a.ZipCode);
            });

            entity.OwnsOne(e => e.Payment, payment =>
            {
                payment.Property(p => p.CardName);
                payment.Property(p => p.CardNumber);
                payment.Property(p => p.Expiration);
                payment.Property(p => p.Cvv);
            });

            entity.Property(e => e.Status);

            entity.HasMany(e => e.OrderItems)
                .WithOne()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired();
        });
    }
}
