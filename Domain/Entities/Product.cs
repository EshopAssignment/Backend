

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    [NotMapped]
    public string? PrimaryImageUrl => 
        Images.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder).Select(x => x.CardUrl).FirstOrDefault();

    public decimal PriceExVat { get; set; }
    public VatRate VatRate { get; set; } = Enums.VatRate.Vat25;
    public ProductCondition Condition { get; set; }
    public ProductType PalletType { get; set; }

    public int OnHand { get; set; }
    public int Reserved { get; set; }
    public int LowStockThreshold { get; set; } = 20;

    [NotMapped]
    public int Available => Math.Max(0, OnHand - Reserved);

    [NotMapped]
    public StockStatus StockStatus =>
        Available <= 0 ? StockStatus.OutOfStock :
        Available <= LowStockThreshold ? StockStatus.LowStock :
        StockStatus.InStock;

    public bool IsActive { get; set; } = true;
    public string? Sku { get; set; } = null!;
    public string? Slug { get; set; } = null!;

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];
}
   
