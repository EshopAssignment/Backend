using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence;

public class PallshoppenDbContext(DbContextOptions<PallshoppenDbContext> options) : DbContext(options), IAppDbContext
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
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).IsRequired();
            entity.Property(p => p.ImgUrl).IsRequired();

            entity.Property(p => p.PriceExVat).HasPrecision(18, 2);
            entity.Property(p => p.VatRate).HasPrecision(5, 4); 

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Product_NonNegative",
                "[OnHand] >= 0 AND [Reserved] >= 0 AND [LowStockThreshold] >= 0"
            ));

            entity.HasIndex(p => new { p.IsActive, p.Name })
                  .HasDatabaseName("IX_Products_IsActive_Name");

            entity.Property(p => p.Sku).HasMaxLength(100);
            entity.Property(p => p.Slug).HasMaxLength(200);

            entity.HasIndex(p => p.Slug).IsUnique().HasFilter("[Slug] IS NOT NULL");
            entity.HasIndex(p => p.Sku).IsUnique().HasFilter("[Sku] IS NOT NULL");

            entity.HasIndex(p => new { p.IsActive, p.Sku })
                  .HasDatabaseName("IX_Products_IsActive_Sku");
            entity.HasIndex(p => new { p.IsActive, p.Slug })
                  .HasDatabaseName("IX_Products_IsActive_Slug");
        });

        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.Property(r => r.CartId).HasMaxLength(64).IsRequired();
            entity.Property(r => r.IdempotencyKey).HasMaxLength(64);

            entity.HasIndex(r => new { r.ProductId, r.Status });
            entity.HasIndex(r => r.IdempotencyKey)
                  .IsUnique()
                  .HasFilter("[IdempotencyKey] IS NOT NULL");

            entity.HasOne(r => r.Product)
                  .WithMany()
                  .HasForeignKey(r => r.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(ConfigureOrder);
        modelBuilder.Entity<OrderItem>(ConfigureOrderItem);
    }

    private static void ConfigureOrder(EntityTypeBuilder<Order> entity)
    {
        entity.HasIndex(o => o.OrderNumber).IsUnique();

        entity.Property(o => o.OrderNumber).HasMaxLength(32).IsRequired();

        entity.Property(o => o.CustomerFirstName).HasMaxLength(100).IsRequired();
        entity.Property(o => o.CustomerLastName).HasMaxLength(100).IsRequired();
        entity.Property(o => o.CustomerEmail).HasMaxLength(200).IsRequired();
        entity.Property(o => o.CustomerPhoneNumber).HasMaxLength(100).IsRequired();

        entity.Property(o => o.OrderStatus).HasConversion<int>();

        
        entity.Property(o => o.Currency).HasMaxLength(3).IsRequired();
        entity.Property(o => o.ProductsSubtotal).HasPrecision(18, 2);
        entity.Property(o => o.ShippingCost).HasPrecision(18, 2);
        entity.Property(o => o.TaxTotal).HasPrecision(18, 2);
        entity.Property(o => o.GrandTotal).HasPrecision(18, 2);

        entity.OwnsOne(o => o.ShippingAddress, a =>
        {
            a.Property(p => p.Street).HasMaxLength(200).HasColumnName("ShippingStreet");
            a.Property(p => p.PostalCode).HasMaxLength(20).HasColumnName("ShippingPostalCode");
            a.Property(p => p.City).HasMaxLength(100).HasColumnName("ShippingCity");
            a.Property(p => p.Country).HasMaxLength(100).HasColumnName("ShippingCountry");
        });

        entity.OwnsOne(o => o.Payment, p =>
        {
            p.Property(x => x.Status).HasConversion<int>();
            p.Property(x => x.Currency).HasMaxLength(3);

            p.Property(x => x.PaymentIntentId).HasMaxLength(128);
            p.Property(x => x.LatestChargeId).HasMaxLength(128);
            p.Property(x => x.PaymentMethodType).HasMaxLength(32);

            p.Property(x => x.AmountAuthorized).HasPrecision(18, 2);
            p.Property(x => x.AmountCaptured).HasPrecision(18, 2);
            p.Property(x => x.AmountRefunded).HasPrecision(18, 2);
        });

        entity.HasMany(o => o.OrderItems)
              .WithOne(i => i.Order)
              .HasForeignKey(i => i.OrderId)
              .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureOrderItem(EntityTypeBuilder<OrderItem> entity)
    {
        entity.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        entity.Property(i => i.Sku).HasMaxLength(64);

        entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
        entity.Property(i => i.LineTotal).HasPrecision(18, 2);
        entity.Property(i => i.VatRate).HasPrecision(5, 4);

        entity.HasOne(i => i.Product)
              .WithMany()
              .HasForeignKey(i => i.ProductId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
