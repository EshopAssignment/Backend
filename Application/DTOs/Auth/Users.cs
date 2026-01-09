
namespace Application.DTOs.Auth;

public sealed record MeDto(
    int UserId,
    string Email,
    string? DisplayName,
    UserProfileDto Profile
    );
public sealed record UserProfileDto(
    string FristName,
    string LastName,
    string Phone,
    int? DefaultShippingAddressId,
    IReadOnlyList<UserAddressDto> Addresses
    );
public sealed record UserAddressDto(
    int Id,
    string Label,
    string Street,
    string City,
    string PostalCode,
    string Country
    );

public sealed record UpdateProfileDto(
    string FirstName,
    string LastName,
    string Phone,
    int? DefaultShippingAddressId
    );
public sealed record UpsertAddressDto
    (string Label,
    string Street,
    string City,
    string PostalCode,
    string Country
    );
