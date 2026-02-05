
namespace IntegrationTests.Contracts.Products;

public sealed class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ImgUrl { get; set; } = "";

    public decimal PriceExVat { get; set; }
    public int VatRatePercent { get; set; }

    public string PalletType { get; set; } = "";
    public string Condition { get; set; } = "";
    public string StockStatus { get; set; } = "";

    public int OnHand { get; set; }
    public int Reserved { get; set; }
    public int Available { get; set; }

    public bool IsActive { get; set; }
    public string? Sku { get; set; }
    public string? Slug { get; set; }
}
