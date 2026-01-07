
namespace Application.DTOs.Shipping;

public sealed record ServicePointDto(
    string Id,
    string Name,
    string Street,
    string PostalCode,
    string City);
