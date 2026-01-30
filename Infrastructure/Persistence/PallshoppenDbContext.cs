using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
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

            entity.Property(p => p.VatRate)
                .HasConversion<int>()
                .HasDefaultValue(VatRate.Vat25);

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Product_NonNegative",
                "[OnHand] >= 0 AND [Reserved] >= 0 AND [LowStockThreshold] >= 0 AND [PriceExVat] >= 0"
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


            entity.HasIndex(r => new { r.CartId, r.ProductId })
                .IsUnique()
                .HasDatabaseName("UX_StockReservations_Active_Cart_Product")
                .HasFilter($"[Status] = {(int)StockReservationStatus.Active}");

            entity.HasIndex(r => new { r.CartId, r.Status })
                .HasDatabaseName("IX_StockReservations_Cart_Status");

            entity.HasIndex(r => new { r.Status, r.ExpiresAt })
                .HasDatabaseName("IX_StockReservations_Status_ExpiresAt");

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

        entity.Property(o => o.CustomerFirstName).HasMaxLength(100).IsRequired(false);
        entity.Property(o => o.CustomerLastName).HasMaxLength(100).IsRequired(false);
        entity.Property(o => o.CustomerEmail).HasMaxLength(200).IsRequired(false);
        entity.Property(o => o.CustomerPhoneNumber).HasMaxLength(100).IsRequired(false);

        entity.Property(o => o.OrderStatus).HasConversion<int>();

        entity.Property(o => o.UserId).IsRequired(false);
        entity.HasIndex(o => o.UserId);

        entity.Property(o => o.Currency).HasMaxLength(3).IsRequired();
        entity.Property(o => o.ProductsSubtotal).HasPrecision(18, 2);
        entity.Property(o => o.ShippingCost).HasPrecision(18, 2);
        entity.Property(o => o.TrackingNumber).HasMaxLength(100).IsRequired(false);
        entity.Property(o => o.VatTotal).HasPrecision(18, 2);
        entity.Property(o => o.GrandTotal).HasPrecision(18, 2);


        entity.Navigation(o => o.ShippingAddress).IsRequired(false);
        entity.OwnsOne(o => o.ShippingAddress, a =>
        {
            a.Property(p => p.Street).HasMaxLength(200).HasColumnName("ShippingStreet").IsRequired(false);
            a.Property(p => p.PostalCode).HasMaxLength(20).HasColumnName("ShippingPostalCode").IsRequired(false);
            a.Property(p => p.City).HasMaxLength(100).HasColumnName("ShippingCity").IsRequired(false);
            a.Property(p => p.Country).HasMaxLength(100).HasColumnName("ShippingCountry").IsRequired(false);
        });

        entity.OwnsOne(o => o.Payment, p =>
        {
            p.Property(x => x.Status).HasConversion<int>();
            p.Property(x => x.Currency).HasMaxLength(3);

            p.Property(x => x.PaymentIntentId).HasMaxLength(128);
            p.Property(x => x.LatestChargeId).HasMaxLength(128);
            p.Property(x => x.PaymentMethodType).HasMaxLength(32);

            p.Property(pp => pp.AmountAuthorized).IsRequired(false);
            p.Property(pp => pp.AmountCaptured).IsRequired(false);
            p.Property(pp => pp.AmountRefunded).IsRequired(false);
        });

        entity.HasMany(o => o.OrderItems)
              .WithOne(i => i.Order)
              .HasForeignKey(i => i.OrderId)
              .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureOrderItem(EntityTypeBuilder<OrderItem> entity)
    {
        entity.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        entity.Property(i => i.Sku).HasMaxLength(64).IsRequired();

        entity.Property(i => i.UnitPriceExVat).HasPrecision(18, 2);
        entity.Property(i => i.UnitVatAmount).HasPrecision(18, 2);
        entity.Property(i => i.UnitPriceIncVat).HasPrecision(18, 2);

        entity.Property(i => i.LineTotalExVat).HasPrecision(18, 2);
        entity.Property(i => i.LineTotalVat).HasPrecision(18, 2);
        entity.Property(i => i.LineTotalIncVat).HasPrecision(18, 2);

        entity.Property(i => i.VatRatePercent).IsRequired();

        entity.ToTable(t => t.HasCheckConstraint(
            "CK_OrderItem_VatRatePercent_Allowed",
            "[VatRatePercent] IN (6, 12, 25)"
        ));

        entity.ToTable(t => t.HasCheckConstraint(
            "CK_OrderItem_NonNegativeAmounts",
            "[UnitPriceExVat] >= 0 AND [UnitVatAmount] >= 0 AND [UnitPriceIncVat] >= 0 AND " +
            "[LineTotalExVat] >= 0 AND [LineTotalVat] >= 0 AND [LineTotalIncVat] >= 0 AND [Quantity] > 0"
        ));

        entity.HasOne(i => i.Product)
              .WithMany()
              .HasForeignKey(i => i.ProductId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
