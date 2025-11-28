

namespace Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    public string PalletType { get; set; } = null!;
    public string Condition { get; set; } = null!;

    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    public string ImgUrl { get; set; } = null!;
    public bool IsActive { get; set; }

    public string? Sku { get; set; } //nullable change later
    public string? Slug { get; set; } //nullable change later
}
   
