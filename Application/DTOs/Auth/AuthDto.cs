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

public sealed record ForgotPasswordDto(
    [Required, EmailAddress, StringLength(256)]
    string Email
);

public sealed record ResetPasswordDto(
    [Required, EmailAddress, StringLength(256)]
    string Email,

    [Required]
    string Token,

    [Required, StringLength(100, MinimumLength = 8)]
    string NewPassword
);

public sealed record ResendVerificationDto(
    [Required, EmailAddress, StringLength(256)]
    string Email
);

public sealed record ConfirmEmailDto(
    [Required]
    int UserId,

    [Required]
    string Token
);

/// <summary>
/// Response returned after login/refresh.
/// Cookies contain tokens, this only informs the client about expiry.
/// </summary>
public sealed record AuthSessionResponseDto(
    DateTime ExpiresAt
);