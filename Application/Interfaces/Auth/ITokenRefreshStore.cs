namespace Application.Interfaces.Auth;

public interface ITokenRefreshStore
{
    Task StoreAsync(int userId, string refreshToken, DateTime expiresAt);
    Task<bool> ValidateAsync(int userId, string refreshToken);
    Task RevokeAsync(int userId, string refreshToken);
}
