

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Domain.Entities.Identity;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _opt;
    private readonly UserManager<User> _user;
    private readonly ITokenRefreshStore _store;
    private readonly SigningCredentials _creds;
    private readonly TokenValidationParameters _validationParams;

    public TokenService(IOptions<JwtOptions> options, UserManager<User> users, ITokenRefreshStore refreshStore)
    {
        _opt = options.Value;
        _user = users;
        _store = refreshStore;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        _creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        _validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _opt.Issuer,
            ValidAudience = _opt.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }
    public async Task<TokenPair> IssueAsync(User user)
    {
        var now = DateTime.UtcNow;
        var exp = now.AddMinutes(_opt.AccessTokenMinutes);

        var roles = await _user.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: now,
            expires: exp,
            signingCredentials: _creds
        );
        var access = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshBytes = RandomNumberGenerator.GetBytes(64);
        var refresh = Convert.ToBase64String(refreshBytes);
        await _store.StoreAsync(user.Id, refresh, now.AddDays(_opt.RefreshTokenDays));

        return new TokenPair(access, refresh, exp);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, _validationParams, out _);
        }
        catch
        {
            return null;
        }
    }
}
