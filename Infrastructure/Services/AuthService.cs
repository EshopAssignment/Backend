
using System.Text;
using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.ACS;
using Application.Interfaces.Auth;
using Domain.Entities.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public sealed class AuthService(UserManager<User> userManager, 
    SignInManager<User> signInManager, 
    ITokenService tokenService,
    ITokenRefreshStore refreshStore,
    AuthDbContext authDb,
    IEmailOutbox emailOutbox,
    IEmailTemplateRenderer templateRenderer,
    IConfiguration config) : IAuthService
{

    private readonly string _publicBaseUrl =
        config["App:PublicBaseUrl"] ?? throw new InvalidOperationException("Missing App:PublicBaseUrl config");
    private readonly bool _requireEmailConfirmation = 
        config.GetValue("Auth:RequireEmailConfirmation", true);


    public async Task<(bool Ok, IDictionary<string, string[]>? Errors)> RegisterAsync(RegisterDto dto, CancellationToken ct)
    {
        var email = (dto.Email ?? "").Trim().ToLowerInvariant();
        var displayName = (dto.DisplayName ?? "").Trim();

        var user = new User
        {
            UserName = email,
            Email = email,
            DisplayName = displayName
        };

        var res = await userManager.CreateAsync(user, dto.Password);
        if(!res.Succeeded)
        {
            var errors = res.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
     
            return (false, errors);
        }

        await userManager.AddToRoleAsync(user, "User");

        var exists = await authDb.UserProfiles.AsNoTracking().AnyAsync(x => x.UserId == user.Id, ct);
        if (exists)
        {
            authDb.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                FirstName = "",
                LastName = "",
                Phone = "",
                DefaultShippingAddressId = null
            });
            await authDb.SaveChangesAsync(ct);
        }

        await EnqueueEmailVerificationAsync(user, ct);

        return (true, null);
    }
    public async Task<(bool Ok, int? UserId, TokenPair? Pair)> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        var email = (dto.Email ?? "").Trim().ToLowerInvariant();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null ) return (false, null, null);

        if (_requireEmailConfirmation && !user.EmailConfirmed)
            return (false, null, null);

        var res = await signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!res.Succeeded) return (false, null, null);

        var pair = await tokenService.IssueAsync(user);
        return (true, user.Id, pair);  
    }
    public async Task<(bool Ok, int? UserId, TokenPair? Pair)> RefreshAsync(int uid, string refreshToken, CancellationToken ct)
    {
        var ok = await refreshStore.ValidateAsync(uid, refreshToken);
        if(!ok) return (false, null, null);

        await refreshStore.RevokeAsync(uid, refreshToken);

        var user = await userManager.FindByIdAsync(uid.ToString());
        if (user is null) return (false, null, null);

        var pair = await tokenService.IssueAsync(user);
        return (true, user.Id, pair);
    }
    public async Task ResendEmailVerificationAsync(string emailRaw, CancellationToken ct)
    {
        var email = (emailRaw ?? "").Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email);

        if (user is null) return;
        if (user.EmailConfirmed) return;

        await EnqueueEmailVerificationAsync(user, ct);
    }
    public async Task<(bool Ok, string? Error)> ConfirmEmailAsync(int userId, string tokenEncoded, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if(user is null) return (false, "User not found");

        var token = DecodeToken(tokenEncoded);
        var res = await userManager.ConfirmEmailAsync(user, token);

        if (!res.Succeeded) return (false, "Invalid Request");

        return (true, null);

    }
    public async Task ForgotPasswordAsync(string emailRaw, CancellationToken ct)
    {
        var email = (emailRaw ?? "").Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email);

        if (user is null) return;

        if(_requireEmailConfirmation && !user.EmailConfirmed)
        {
            await EnqueueEmailVerificationAsync(user, ct);
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var tokenEncoded = EncodeToken(token);

        var resetUrl = $"{_publicBaseUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={tokenEncoded}";
        var html = templateRenderer.RenderPassawordReset(resetUrl);

        const string kind = "PasswordReset";
        var correlationId = $"{user.Id}:{kind}:{DateTime.UtcNow:yyyyMMddHHmm}";

        await emailOutbox.EnqueueAsync(
            to:email,
            subject: "Återställ ditt lösenrod",
            htmlBody: html,
            kind: kind,
            correlationId: correlationId,
            ct: ct);

    }
    public async Task<(bool Ok, string? Error)> ResetPasswordAsync(string emailRaw, string tokenEncoded, string newPassword, CancellationToken ct)
    {
        var email = (emailRaw ?? "").Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return (false, "User not found");

        var token = DecodeToken(tokenEncoded);
        var res = await userManager.ResetPasswordAsync(user, token, newPassword);

        if (!res.Succeeded) return (false, "Invalid Request");
        return (true, null);
    }
    private async Task EnqueueEmailVerificationAsync(User user, CancellationToken ct)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var tokenEncoded = EncodeToken(token);

        var verifyUrl = $"{_publicBaseUrl}/verify-email?userId={user.Id}&token={tokenEncoded}";
        var html = templateRenderer.RenderEmailVerification(verifyUrl);

        const string kind = "email_verification";
        var correlationId = $"{user.Id}:{kind}";

        await emailOutbox.EnqueueAsync(
            to: user.Email!,
            subject: "Bekräfta din e-post",
            htmlBody: html,
            kind: kind,
            correlationId: correlationId,
            ct: ct);
    }
    private static string EncodeToken(string token)
        => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    private static string DecodeToken(string tokenEncoded)
        => Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(tokenEncoded));
}
