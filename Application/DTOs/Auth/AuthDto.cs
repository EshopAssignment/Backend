
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

public sealed record ForgotPasswordDto(string Email);
public sealed record ResetPasswordDto(string Email, string Token, string NewPassword);
public sealed record ResendVerificationDto(string Email);
public sealed record ConfirmEmailDto(int UserId, string Token);


