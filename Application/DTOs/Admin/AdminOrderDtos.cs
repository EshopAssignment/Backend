

namespace Application.DTOs.Admin;

public sealed record AdminOrderListItemDto(
    int Id,
    string OrderNumber,
    DateTime OrderDate,
    string CustomerName,
    string CustomerEmail,
    string OrderStatus,
    decimal Total
    );
public sealed record AdminOrderItemDto(
    int ProductId,
    string ProductName,
    decimal Price,
    int Quantity,
    decimal LineTotal);
public sealed record AdminOrderDetailsDto(

    int Id,
    string OrderNumber,
    DateTime OrderDate,
    string CustomerFirstName,
    string CustomerLastName,
    string CustomerEmail,
    string CustomerPhoneNumber,
    string ShippingStreet,
    string ShippingPostalCode,
    string ShippingCity,
    string ShippingCountry,
    string OrderStatus,
    decimal ProductsTotal,
    decimal ShippingCost,
    decimal Total,
    IReadOnlyList<AdminOrderItemDto> Items

);

public sealed record AdminUpdateOrderStatusRequest(string OrderStatus);

