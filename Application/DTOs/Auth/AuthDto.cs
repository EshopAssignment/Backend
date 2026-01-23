
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public sealed record LoginDto(
    [Required, EmailAddress, StringLength(256)]
    string Email,

    [Required, StringLength(100, MinimumLength = 8)]
    string Password
);

public sealed record RegisterDto(
    [Required, EmailAddress, StringLength(256)]
    string Email,

    [Required, StringLength(50, MinimumLength = 2)]
    string DisplayName,

    [Required, StringLength(100, MinimumLength = 8)]
    string Password
);



