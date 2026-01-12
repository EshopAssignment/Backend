using System.Formats.Asn1;
using Application.DTOs.Auth;
using Domain.Entities.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MeController(UserManager<User> users, AuthDbContext authContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MeDto>> GetProfile(CancellationToken ct)
    {
        var uid = int.Parse(users.GetUserId(User)!);

        var u = await authContext.Users
            .AsNoTracking()
            .Include(x => x.Profile)
            .ThenInclude(p => p.Addresses)
            .FirstOrDefaultAsync(x => x.Id == uid, ct);

        if (u is null) return Unauthorized();

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
            )
        ));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto, CancellationToken ct)
    {
        var uid = int.Parse(users.GetUserId(User)!);

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
    public async Task<ActionResult> AddAdresses(UpsertAddressDto dto, CancellationToken ct)
    {
        var uid = int.Parse(users.GetUserId(User)!);

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

        return CreatedAtAction(nameof(GetProfile), new { }, null);
    }

    [HttpPatch("profile/default-address")]
    public async Task<IActionResult> SetDefaultAddress(SetDefaultAddressDto dto, CancellationToken ct)
    {
        var uid = int.Parse(users.GetUserId(User)!);

        var u = await authContext.Users.Include(x => x.Profile).FirstOrDefaultAsync(x => x.Id == uid, ct);

        if (u is null) return Unauthorized();

        u.Profile ??= new UserProfile { UserId = u.Id };
        u.Profile.DefaultShippingAddressId = dto.DefaultShippingAddressId;

        await authContext.SaveChangesAsync(ct);
        return NoContent();
    }
}
