

namespace Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set;}    
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public Product Product { get; set; } = null!;

}
