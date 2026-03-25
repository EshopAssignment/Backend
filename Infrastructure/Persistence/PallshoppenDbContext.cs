using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Mail;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence;

public class PallshoppenDbContext(DbContextOptions<PallshoppenDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<EmailOutboxMessage> EmailOutBox => Set<EmailOutboxMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    
    public DbSet<CustomRequest> CustomRequest => Set<CustomRequest>();
    public DbSet<CustomQuote> CustomQuote => Set<CustomQuote>();
    public DbSet<CustomQuoteItem> CustomQuoteItem => Set<CustomQuoteItem>();
   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("core");

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).IsRequired();

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

            entity.HasMany(p => p.Images)
                  .WithOne(i => i.Product)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.Slug).IsUnique().HasFilter("[Slug] IS NOT NULL");
            entity.HasIndex(p => p.Sku).IsUnique().HasFilter("[Sku] IS NOT NULL");

            entity.HasIndex(p => new { p.IsActive, p.Sku })
                  .HasDatabaseName("IX_Products_IsActive_Sku");
            entity.HasIndex(p => new { p.IsActive, p.Slug })
                  .HasDatabaseName("IX_Products_IsActive_Slug");
        });
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImages");

            entity.Property(i => i.Url)
                  .HasMaxLength(2048)
                  .IsRequired();

            entity.Property(i => i.AltText)
                  .HasMaxLength(200);

            entity.Property(i => i.SortOrder).HasDefaultValue(0);
            entity.Property(i => i.IsPrimary).HasDefaultValue(false);

            entity.Property(i => i.CreatedAtUtc)
                  .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(i => new { i.ProductId, i.SortOrder })
                  .HasDatabaseName("IX_ProductImages_ProductId_SortOrder");

            entity.HasIndex(i => new { i.ProductId, i.IsPrimary })
                  .HasDatabaseName("IX_ProductImages_ProductId_IsPrimary");
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
        modelBuilder.Entity<EmailOutboxMessage>(b =>
        {
            b.ToTable("EmailOutbox", "core");
            b.HasKey(x => x.Id);

            b.Property(x => x.To).HasMaxLength(500).IsRequired();
            b.Property(x => x.Subject).HasMaxLength(500).IsRequired();
            b.Property(x => x.Kind).HasMaxLength(100).IsRequired();

            b.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
            b.Property(x => x.Status).HasConversion<int>();
            b.Property(x => x.HtmlBody).IsRequired();

            b.HasIndex(x => new { x.Status, x.NextAttempt });
            b.HasIndex(x => x.CorrelationId);
        });
        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages", "core");
            b.HasKey(x => x.Id);

            b.Property(x => x.Type).HasMaxLength(500).IsRequired();
            b.Property(x => x.CorrelationId).HasMaxLength(500).IsRequired();

            b.HasIndex(x => new { x.PublichedAtUtc, x.CreatedAtUtc });
            b.HasIndex(x => x.CorrelationId).IsUnique();
        });
        modelBuilder.Entity<CustomRequest>(ConfigureCustomRequest);
        modelBuilder.Entity<CustomQuote>(ConfigureCustomQuote);
        modelBuilder.Entity<CustomQuoteItem>(ConfigureCustomQuoteItem);
    }
    private static void ConfigureOrder(EntityTypeBuilder<Order> entity)
    {
        entity.HasKey(o => o.Id);
        entity.Property(o => o.Id)
            .UseIdentityColumn()
            .ValueGeneratedOnAdd();

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
    private static void ConfigureCustomRequest(EntityTypeBuilder<CustomRequest> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Phone).HasMaxLength(100);
        entity.Property(x => x.Message).IsRequired();

        entity.Property(x => x.AttatchemntName).HasMaxLength(260);
        entity.Property(x => x.AttatchemtBlobPath).HasMaxLength(500);

        entity.Property(x => x.InternalNote).HasMaxLength(2000);
        entity.Property(x => x.Status).HasConversion<int>();

        entity.HasIndex(x => x.CreatedAtUtc);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.Email);
    }
    private static void ConfigureCustomQuote(EntityTypeBuilder<CustomQuote> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        entity.Property(x => x.CustomerMessage).HasMaxLength(4000);
        entity.Property(x => x.InternalNote).HasMaxLength(2000);

        entity.Property(x => x.Status).HasConversion<int>();

        entity.Property(x => x.SubtotalExVat).HasPrecision(18, 2);
        entity.Property(x => x.VatTotal).HasPrecision(18, 2);
        entity.Property(x => x.TotalIncVat).HasPrecision(18, 2);

        entity.HasOne(x => x.CustomRequest)
            .WithMany(x => x.Quotes)
            .HasForeignKey(x => x.CustomRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => x.CustomRequestId);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.CreatedAtUtc);
    }
    private static void ConfigureCustomQuoteItem(EntityTypeBuilder<CustomQuoteItem> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
        entity.Property(x => x.VatRatePercent).IsRequired();

        entity.Property(x => x.UnitPriceExVat).HasPrecision(18, 2);
        entity.Property(x => x.UnitVatAmount).HasPrecision(18, 2);
        entity.Property(x => x.UnitPriceIncVat).HasPrecision(18, 2);

        entity.Property(x => x.LineTotalExVat).HasPrecision(18, 2);
        entity.Property(x => x.LineTotalVat).HasPrecision(18, 2);
        entity.Property(x => x.LineTotalIncVat).HasPrecision(18, 2);

        entity.HasOne(x => x.CustomQuote)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CustomQuoteId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.ToTable(t => t.HasCheckConstraint(
            "CK_CustomQuoteItem_NonNegativeAmounts",
            "[UnitPriceExVat] >= 0 AND [UnitVatAmount] >= 0 AND [UnitPriceIncVat] >= 0 AND " +
            "[LineTotalExVat] >= 0 AND [LineTotalVat] >= 0 AND [LineTotalIncVat] >= 0 AND [Quantity] > 0"
        ));

        entity.ToTable(t => t.HasCheckConstraint(
            "CK_CustomQuoteItem_VatRatePercent_Allowed",
            "[VatRatePercent] IN (6, 12, 25)"
        ));
    }
}
