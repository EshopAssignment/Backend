
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public sealed record MeDto(
    int UserId,
    string Email,
    string? DisplayName,
    UserProfileDto Profile,
    IReadOnlyList<string> Roles
    );
public sealed record UserProfileDto(
    string FirstName,
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

    [property: Required, StringLength(50, MinimumLength = 1)]
    string FirstName,

    [property: Required, StringLength(50, MinimumLength = 1)]
    string LastName,

    [property: RegularExpression(@"^$|^[0-9+\-\s]{6,20}$", ErrorMessage = "Phone must be valid.")]
    string? Phone,

    int? DefaultShippingAddressId
    );

public sealed record UpsertAddressDto(

    [property: StringLength(50)]
    string? Label,

    [property: Required, StringLength(60, MinimumLength = 1)]
    string Street,

    [property: Required, StringLength(60, MinimumLength = 1)]
    string City,

    [property: Required, RegularExpression(@"^\d{5}$", ErrorMessage = "PostalCode must be 5 digits.")]
    string PostalCode,

    [property: RegularExpression(@"^[A-Za-z]{2}$", ErrorMessage = "Country must land code")]
    string? Country
    );
public sealed record SetDefaultAddressDto(int? DefaultShippingAddressId);