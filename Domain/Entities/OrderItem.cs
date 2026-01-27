

namespace Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set;}    
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public string ProductName { get; set; } = null!;

    public int VatRatePercent { get; set; }
    public decimal UnitPriceExVat { get; set; }
    public decimal UnitVatAmount { get; set; }
    public decimal UnitPriceIncVat { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotalExVat { get; set; }
    public decimal LineTotalVat {  get; set; }
    public decimal LineTotalIncVat { get; set; }

    public Product Product { get; set; } = null!;

}
