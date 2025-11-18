using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class PallshoppenDbContext(DbContextOptions<PallshoppenDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(p => p.PalletType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(p => p.Condition)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.OrderNumber)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(o => o.CustomerFirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.CustomerLastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.CustomerEmail)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(o => o.CustomerPhoneNumber)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.ShippingStreet)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(o => o.ShippingPostalCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(o => o.ShippingCity)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.ShippingCountry)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.ProductsTotal)
                .HasColumnType("decimal(18,2)");

            entity.Property(o => o.ShippingCost)
                .HasColumnType("decimal(18,2)");

            entity.Property(o => o.Total)
                .HasColumnType("decimal(18,2)");

            entity.HasMany(o => o.OrderItems)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(i => i.LineTotal)
                .HasColumnType("decimal(18,2)");
        });
    }

}
