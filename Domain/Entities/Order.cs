

using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Customer Information
    public string? CustomerFirstName { get; set; }
    public string? CustomerLastName { get; set; } 
    public string? CustomerEmail { get; set; } 
    public string? CustomerPhoneNumber { get; set; } 
    
    public void SetCustomerEmail(string email)
    {
        CustomerEmail = email;
        Touch();
    }
    public void SetCustomer(string firstName, string lastName, string email, string? phone)
    {
        CustomerFirstName = firstName;
        CustomerLastName = lastName;
        CustomerEmail = email;
        CustomerPhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Touch();
    }
    public void ClearCustomer()
    {
        CustomerFirstName = null;
        CustomerLastName = null;
        CustomerEmail = null;
        CustomerPhoneNumber = null;
        Touch();
    }

    // Shipping Information
    public ShippingAddress? ShippingAddress { get; set; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.None;
    public ShippingCarrier ShippingCarrier { get; set; } = ShippingCarrier.None;
    public string? ServicePointId { get; private set; }
    public string? ServicePointName { get; private set; }
    public string? ServicePointAddress { get; private set; }
    public string? TrackingNumber { get; private set; }

    public void SetTracking(string trackingNumber)
    {
        TrackingNumber = string.IsNullOrWhiteSpace(trackingNumber) ? null : trackingNumber.Trim();
        Touch();
    }

    public void ClearTrackingNumber()
    {
        TrackingNumber = null;
        Touch();
    }

    //Currency and Totals
    public string Currency { get; private set; } = "SEK";
    public decimal ProductsSubtotal { get; private set; }  
    public decimal ShippingCost { get; private set; }
    public decimal VatTotal { get; private set; }
    public decimal GrandTotal { get; private set; }

    //Cart Association
    public string CartId { get; private set; } = null!;
    public void SetCartId(string cartId) => CartId = cartId;


    // Order Items
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public OrderPayment Payment { get; private set; } = OrderPayment.Init("SEK");
    public Order() { }
    public Order(string orderNumber, string currency = "SEK")
    {
        OrderNumber = orderNumber;
        Currency = currency;
        Payment = OrderPayment.Init(currency);
    }

    // Methods to manage order items and recalculate totals
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

    private void RecalculateTotals()
    {
        ProductsSubtotal = OrderItems.Sum(i => i.LineTotalExVat);
        VatTotal = OrderItems.Sum(i => i.LineTotalVat);
        GrandTotal = ProductsSubtotal + VatTotal + ShippingCost;
        Touch();
    }

    // Order Status Management
    public void MarkConfirmed() { OrderStatus = OrderStatus.Confirmed; Touch(); }
    public void MarkProcessing() { OrderStatus = OrderStatus.Processing; Touch(); }
    public void MarkShipped() { OrderStatus = OrderStatus.Shipped; Touch(); }
    public void MarkCompleted() { OrderStatus = OrderStatus.Completed; Touch(); }
    public void MarkCancelled() { OrderStatus = OrderStatus.Cancelled; Touch(); }
    public void MarkFailed() { OrderStatus = OrderStatus.Failed; Touch(); }
    public void MarkRefunded() { OrderStatus = OrderStatus.Refunded; Touch(); }
    private void Touch() => UpdatedAt = DateTime.UtcNow;

    //User Id Management
    public int? UserId { get; set; }
    public void SetUserId(int? userId) => UserId = userId;

    // Shipping Information Management

    public void SetShippingAddress(ShippingAddress address) { ShippingAddress = address; Touch(); }
    public void ClearShippingAddress()
    {
        ShippingAddress = null;
        Touch();
    }
    public void SetShippingSelection(ShippingCarrier carrier, ShippingMethod method, decimal cost, string? servicePointId = null, string? sericePointName = null, string? servicePointAdress =null)
    {
        ShippingCarrier = carrier;
        ShippingMethod = method;
        ServicePointId = servicePointId;
        ServicePointName = sericePointName;
        ServicePointAddress = servicePointAdress;
        SetShippingCost(cost);
    }


    //readiness checks for gating
    [NotMapped]

    public bool CustomerReady => 
        !string.IsNullOrWhiteSpace(CustomerFirstName)
        && !string.IsNullOrWhiteSpace(CustomerLastName)
        && !string.IsNullOrWhiteSpace(CustomerEmail);

    [NotMapped]

    public bool AddressReady =>
        ShippingAddress is not null
        && !string.IsNullOrWhiteSpace(ShippingAddress.Street)
        && !string.IsNullOrWhiteSpace(ShippingAddress.City)
        && !string.IsNullOrWhiteSpace(ShippingAddress.PostalCode)
        && !string.IsNullOrWhiteSpace(ShippingAddress.Country);

    [NotMapped]

    public bool ShippingSelected =>
        ShippingCarrier != ShippingCarrier.None
        && ShippingMethod != ShippingMethod.None;
}