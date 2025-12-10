

using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string CustomerFirstName { get; set; } = null!;
    public string CustomerLastName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhoneNumber { get; set; } = null!;

    public ShippingAddress ShippingAddress { get; set; } = null!;
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

    public string Currency { get; private set; } = "SEK";
    public decimal ProductsSubtotal { get; private set; }  
    public decimal ShippingCost { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal GrandTotal { get; private set; }


    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public OrderPayment Payment { get; private set; } = OrderPayment.Init("SEK");
    public Order() { }
    public Order(string orderNumber, ShippingAddress shippingAdress, string currency = "SEK")
    {
        OrderNumber = orderNumber;
        ShippingAddress = shippingAdress;
        Currency = currency;
        Payment = OrderPayment.Init(currency);
    }

    public void ReplaceItems(IEnumerable<OrderItem> items)
    {
        OrderItems.Clear();
        foreach (var i in items) OrderItems.Add(i);
        RecalculateTotals();
    }
    public void SetShippingCost(decimal cost)
    {
        ShippingCost = cost;
        RecalculateTotals();
    }
    public void SetTaxTotal(decimal tax)
    {
        TaxTotal = tax;
        RecalculateTotals();
    }
    private void RecalculateTotals()
    {
        ProductsSubtotal = OrderItems.Sum(i => i.LineTotal);
        GrandTotal = ProductsSubtotal + ShippingCost + TaxTotal;
        Touch();
    }
    public void MarkConfirmed() { OrderStatus = OrderStatus.Confirmed; Touch(); }
    public void MarkProcessing() { OrderStatus = OrderStatus.Processing; Touch(); }
    public void MarkShipped() { OrderStatus = OrderStatus.Shipped; Touch(); }
    public void MarkCompleted() { OrderStatus = OrderStatus.Completed; Touch(); }
    public void MarkCancelled() { OrderStatus = OrderStatus.Cancelled; Touch(); }
    public void MarkFailed() { OrderStatus = OrderStatus.Failed; Touch(); }
    public void MarkRefunded() { OrderStatus = OrderStatus.Refunded; Touch(); }
    private void Touch() => UpdatedAt = DateTime.UtcNow;

    public void SetShippingAddress(ShippingAddress address) { ShippingAddress = address; Touch(); }
}