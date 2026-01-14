

namespace Application.DTOs.Order;

public sealed record UpdateOrderCustomerDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone);

public sealed record UpdateOrderShippingAddressDto(
    string Street,
    string City,
    string PostalCode,
    string Country = "SE");
