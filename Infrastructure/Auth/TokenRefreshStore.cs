using Application.Interfaces.Auth;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Auth;

public class TokenRefreshStore(IMemoryCache cache) : ITokenRefreshStore
{
    private readonly IMemoryCache _cache = cache;

    private static string Key(int userId, string token) => $"rt:{userId}:{token}";

    public Task StoreAsync(int userId, string refreshToken, DateTime expiresAt)
    {
        _cache.Set(Key(userId, refreshToken), true, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiresAt
        });
        return Task.CompletedTask;
    }

    public Task<bool> ValidateAsync(int userId, string refreshToken)
        => Task.FromResult(_cache.TryGetValue(Key(userId, refreshToken), out _));

    public Task RevokeAsync(int userId, string refreshToken)
    {
        _cache.Remove(Key(userId, refreshToken));
        return Task.CompletedTask;
    }

}
