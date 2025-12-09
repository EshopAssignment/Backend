using System.Security.Claims;
using Application.DTOs.Auth;
using Domain.Entities.Identity;

namespace Application.Interfaces.Auth;

public interface ITokenService
{
    Task<TokenPair> IssueAsync(User user);
    ClaimsPrincipal? ValidateAccessToken(string token);
}
