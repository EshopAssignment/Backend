using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class PallshoppenDbContext(DbContextOptions<PallshoppenDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("core");

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(p => p.Description)
                .IsRequired();

            entity.Property(p => p.ImgUrl)
                .IsRequired();

            entity.Property(p => p.PriceExVat)
                .HasPrecision(18, 2);

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Product_NonNegative",
                "[OnHand] >= 0 AND [Reserved] >= 0 AND [LowStockThreshold] >= 0"
            ));

            entity.HasIndex(p => new { p.IsActive, p.Name })
                  .HasDatabaseName("IX_Products_IsActive_Name");

            entity.Property(p => p.Sku).HasMaxLength(100);
            entity.Property(p => p.Slug).HasMaxLength(200);

            entity.HasIndex(p => p.Slug)
                  .IsUnique()
                  .HasFilter("[Slug] IS NOT NULL");
            entity.HasIndex(p => p.Sku)
                  .IsUnique()
                  .HasFilter("[Sku] IS NOT NULL");

            // filterindex
            entity.HasIndex(p => new { p.IsActive, p.Sku })
                  .HasDatabaseName("IX_Products_IsActive_Sku");
            entity.HasIndex(p => new { p.IsActive, p.Slug })
                  .HasDatabaseName("IX_Products_IsActive_Slug");
        });

        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.Property(r => r.CartId)
                  .HasMaxLength(64)
                  .IsRequired();

            entity.Property(r => r.IdempotencyKey)
                  .HasMaxLength(64);

            // Snabba upp status- och produktfrågor
            entity.HasIndex(r => new { r.ProductId, r.Status });

            // En idempotency-nyckel används bara en gång
            entity.HasIndex(r => r.IdempotencyKey)
                  .IsUnique()
                  .HasFilter("[IdempotencyKey] IS NOT NULL");

            // FK till Product
            entity.HasOne(r => r.Product)
                  .WithMany()
                  .HasForeignKey(r => r.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

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

            entity.Property(o => o.ProductsTotal).HasPrecision(18, 2);
            entity.Property(o => o.ShippingCost).HasPrecision(18, 2);
            entity.Property(o => o.Total).HasPrecision(18, 2);

            entity.HasMany(o => o.OrderItems)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(o => o.OrderStatus)
                .HasMaxLength(32)
                .IsRequired();
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.Property(i => i.LineTotal).HasPrecision(18, 2);

            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

}
