

namespace Application.DTOs.Auth;

public sealed record TokenPair(
    string AccessToken, string RefreshToken, DateTime ExpiresAt
    );
