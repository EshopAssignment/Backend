using System.Diagnostics.Eventing.Reader;
using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<(bool Ok, IDictionary<string, string[]>? Errors)> RegisterAsync(RegisterDto dto, CancellationToken ct);
    Task<(bool Ok, int? UserId, TokenPair? Pair)> LoginAsync(LoginDto dto, CancellationToken ct);
    Task<(bool Ok, int? UserId, TokenPair? Pair)> RefreshAsync(int uid, string refreshToken, CancellationToken ct);

    Task ForgotPasswordAsync(string email, CancellationToken ct);
    Task<(bool Ok, String? Error)> ResetPasswordAsync(string email, string toke, string newPassword, CancellationToken ct);

    Task ResendEmailVerificationAsync(string email, CancellationToken ct);
    Task<(bool Ok, string? Error)> ConfirmEmailAsync(int userId, string token, CancellationToken ct);
}
