using System.Formats.Asn1;
using Application.DTOs.Auth;
using Domain.Entities.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public class MeController(UserManager<User> users, AuthDbContext authContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MeDto>> GetProfile(CancellationToken ct)
    {
        if (!int.TryParse(users.GetUserId(User), out var uid))
            return Unauthorized();

        var u = await authContext.Users
            .AsNoTracking()
            .Include(x => x.Profile)
            .ThenInclude(p => p.Addresses)
            .FirstOrDefaultAsync(x => x.Id == uid, ct);

        if (u is null) return Unauthorized();

        var roles = await users.GetRolesAsync(u);

        var p = u.Profile ?? new UserProfile { UserId = u.Id };

        return Ok(new MeDto(
            u.Id,
            u.Email ?? "",
            u.DisplayName,
            new UserProfileDto(
                p.FirstName,
                p.LastName,
                p.Phone,
                p.DefaultShippingAddressId,
                p.Addresses
                    .Where(a => !a.IsDeleted)
                    .Select(a => new UserAddressDto(a.Id, a.Label, a.Street, a.City, a.PostalCode, a.Country))
                    .ToList()
            ),
            roles.ToList()
        ));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto, CancellationToken ct)
    {
        if (!int.TryParse(users.GetUserId(User), out var uid))
            return Unauthorized();

        var u = await authContext.Users
            .Include(x => x.Profile)
            .ThenInclude(p => p.Addresses)
            .FirstOrDefaultAsync(x => x.Id == uid, ct);

        if (u is null) return Unauthorized();

        u.Profile ??= new UserProfile { UserId = u.Id };

        u.Profile.FirstName = dto.FirstName?.Trim() ?? "";
        u.Profile.LastName = dto.LastName?.Trim() ?? "";
        u.Profile.Phone = dto.Phone?.Trim() ?? "";
        u.Profile.DefaultShippingAddressId = dto.DefaultShippingAddressId;

        await authContext.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("addresses")]
    public async Task<IActionResult> AddAddress(UpsertAddressDto dto, CancellationToken ct)
    {
        if (!int.TryParse(users.GetUserId(User), out var uid))
            return Unauthorized();

        var u = await authContext.Users
            .Include(x => x.Profile)
            .ThenInclude(p => p.Addresses)
            .FirstOrDefaultAsync(x => x.Id == uid, ct);

        if (u is null) return Unauthorized();

        u.Profile ??= new UserProfile { UserId = u.Id };

        var a = new UserAddress
        {
            UserId = u.Id,
            Label = string.IsNullOrWhiteSpace(dto.Label) ? "Home" : dto.Label.Trim(),
            Street = dto.Street?.Trim() ?? "",
            City = dto.City?.Trim() ?? "",
            PostalCode = dto.PostalCode?.Trim() ?? "",
            Country = (dto.Country?.Trim() ?? "SE").ToUpperInvariant()
        };

        u.Profile.Addresses.Add(a);
        await authContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPatch("profile/default-address")]
    public async Task<IActionResult> SetDefaultAddress(SetDefaultAddressDto dto, CancellationToken ct)
    {
        if (!int.TryParse(users.GetUserId(User), out var uid))
            return Unauthorized();

        var u = await authContext.Users
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.Id == uid, ct);

        if (u is null) return Unauthorized();

        u.Profile ??= new UserProfile { UserId = u.Id };
        u.Profile.DefaultShippingAddressId = dto.DefaultShippingAddressId;

        await authContext.SaveChangesAsync(ct);
        return NoContent();
    }
}
