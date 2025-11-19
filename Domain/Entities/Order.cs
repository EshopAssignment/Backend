

namespace Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string CustomerFirstName { get; set; } = null!;
    public string CustomerLastName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhoneNumber { get; set; } = null!;
    
    public string ShippingStreet { get; set; } = null!;
    public string ShippingCity { get; set; } = null!;
    public string ShippingPostalCode { get; set; } = null!;
    public string ShippingCountry { get; set; } = null!;

    public string OrderStatus { get; set; } = "New";

    public decimal ProductsTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }


    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
