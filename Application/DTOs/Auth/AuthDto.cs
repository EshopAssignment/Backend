
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public sealed record LoginDto(
    [property: Required, EmailAddress, StringLength(256)]
    string Email,

    [property: Required]
    string Password
);

public sealed record RegisterDto(
    [property: Required, EmailAddress, StringLength(256)]
    string Email,

    [property: Required, StringLength(50, MinimumLength = 2)]
    string DisplayName,

    [property: Required, StringLength(100, MinimumLength = 8)]
    string Password
);



