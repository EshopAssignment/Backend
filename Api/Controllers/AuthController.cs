using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController, Route("auth")]
public class AuthController(IAuthService auth): ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken ct)
    {
        var (ok, errors) = await auth.RegisterAsync(dto, ct);
        if (!ok) return ValidationProblem(new ValidationProblemDetails(errors!));
        return Ok();
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthSessionResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var (ok, uid, pair) = await auth.LoginAsync(dto, ct);
        if (!ok || uid is null || pair is null) return Unauthorized();

        WriteCookies(uid.Value, pair);
        return Ok(new AuthSessionResponseDto(pair.ExpiresAt));
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthSessionResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var uidCookie = Request.Cookies["uid"];
        var refresh = Request.Cookies["refresh_token"];

        if (!int.TryParse(uidCookie, out var uid) || string.IsNullOrEmpty(refresh))
            return Unauthorized();

        var (ok, userId, pair) = await auth.RefreshAsync(uid, refresh, ct);
        if (!ok || userId is null || pair is null) return Unauthorized();

        WriteCookies(userId.Value, pair);
        return Ok(new AuthSessionResponseDto(pair.ExpiresAt));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken ct)
    {
        await auth.ForgotPasswordAsync(dto.Email, ct);
        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct)
    {
        var (ok, err) = await auth.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword, ct);
        if (!ok) return BadRequest(new { message = err });
        return Ok();
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto, CancellationToken ct)
    {
        await auth.ResendEmailVerificationAsync(dto.Email, ct);
        return Ok();
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto, CancellationToken ct)
    {
        var (ok, err) = await auth.ConfirmEmailAsync(dto.UserId, dto.Token, ct);
        if (!ok) return BadRequest(new { message = err });
        return Ok();
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
