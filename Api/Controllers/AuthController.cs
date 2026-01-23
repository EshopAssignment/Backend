using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Domain.Entities.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController, Route("auth")]
public class AuthController : ControllerBase
{
    public readonly UserManager<User> _user;
    public readonly SignInManager<User> _signInManager;
    public readonly ITokenService _tokenService;
    public readonly ITokenRefreshStore _refreshStore;
    public readonly AuthDbContext _authContext;

    public AuthController(UserManager<User> user, SignInManager<User> manager, ITokenService tokenService, ITokenRefreshStore refreshStore, AuthDbContext authContext)
        => (_user, _signInManager, _tokenService, _refreshStore, _authContext) = (user, manager, tokenService, refreshStore, authContext);


    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = new User
        {
            UserName = email,
            Email = email,
            DisplayName = dto.DisplayName.Trim()
        };

        var res = await _user.CreateAsync(user, dto.Password);
        if (!res.Succeeded)
            return ValidationProblem(new ValidationProblemDetails(
                res.Errors.GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray())
            ));

        await _user.AddToRoleAsync(user, "User");

        var exists = await _authContext.UserProfiles.FindAsync(user.Id);
        if (exists is null)
        {
            _authContext.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                FirstName = "",
                LastName = "",
                Phone = "",
                DefaultShippingAddressId = null
            });

            await _authContext.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _user.FindByEmailAsync(email);
        if (user is null) return Unauthorized();

        var res = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!res.Succeeded) return Unauthorized();

        var pair = await _tokenService.IssueAsync(user);
        WriteCookies(user.Id, pair);
        return Ok(new { expiresAt = pair.ExpiresAt });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        Response.Cookies.Delete("uid");
        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var uidCookie = Request.Cookies["uid"];
        var refresh = Request.Cookies["refresh_token"];
        if (!int.TryParse(uidCookie, out var uid) || string.IsNullOrEmpty(refresh)) return Unauthorized();

        var ok = await _refreshStore.ValidateAsync(uid, refresh);
        if (!ok) return Unauthorized();

        await _refreshStore.RevokeAsync(uid, refresh);

        var user = await _user.FindByIdAsync(uid.ToString());
        if (user is null) return Unauthorized();

        var pair = await _tokenService.IssueAsync(user);
        WriteCookies(uid, pair);
        return Ok(new { expiresAt = pair.ExpiresAt });
    }

    void WriteCookies(int userId, TokenPair p)
    {
        var baseOpt = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        };

        var accessOpt = new CookieOptions
        {
            HttpOnly = baseOpt.HttpOnly,
            Secure = baseOpt.Secure,
            SameSite = baseOpt.SameSite,
            Expires = p.ExpiresAt
        };
        Response.Cookies.Append("access_token", p.AccessToken, accessOpt);

        var refreshOpt = new CookieOptions
        {
            HttpOnly = baseOpt.HttpOnly,
            Secure = baseOpt.Secure,
            SameSite = baseOpt.SameSite,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refresh_token", p.RefreshToken, refreshOpt);

        var uidOpt = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("uid", userId.ToString(), uidOpt);
    }
}
