using System.Security.Claims;
using Application.DTOs.Auth;
using Application.DTOs.Order;
using Application.Interfaces;
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
public class MeController(UserManager<User> users, AuthDbContext authContext, IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MeDto>> GetProfile(CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null) return Unauthorized();

        var u = await authContext.Users
            .AsNoTracking()
            .Include(x => x.Profile)
            .ThenInclude(p => p.Addresses)
            .FirstOrDefaultAsync(x => x.Id == uid.Value, ct);

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
        var uid = GetUserId();
        if (uid is null) return Unauthorized();

        var u = await authContext.Users
            .Include(x => x.Profile)
            .ThenInclude(p => p.Addresses!)
            .FirstOrDefaultAsync(x => x.Id == uid.Value, ct);

        if (u is null) return Unauthorized();

        u.Profile ??= new UserProfile { UserId = u.Id };

        u.Profile.FirstName = dto.FirstName.Trim();
        u.Profile.LastName = dto.LastName.Trim();
        u.Profile.Phone = (dto.Phone ?? "").Trim();
        u.Profile.DefaultShippingAddressId = dto.DefaultShippingAddressId;

        await authContext.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("addresses")]
    public async Task<IActionResult> AddAddress(UpsertAddressDto dto, CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null) return Unauthorized();

        var u = await authContext.Users
            .Include(x => x.Profile)
            .ThenInclude(p => p.Addresses!)
            .FirstOrDefaultAsync(x => x.Id == uid.Value, ct);

        if (u is null) return Unauthorized();

        u.Profile ??= new UserProfile { UserId = u.Id };

        var postal = (dto.PostalCode ?? "").Trim().Replace(" ", "");

        var a = new UserAddress
        {
            UserId = u.Id,
            Label = string.IsNullOrWhiteSpace(dto.Label) ? "Home" : dto.Label.Trim(),
            Street = dto.Street?.Trim() ?? "",
            City = dto.City?.Trim() ?? "",
            PostalCode = postal,
            Country = (dto.Country?.Trim() ?? "SE").ToUpperInvariant()
        };

        u.Profile.Addresses.Add(a);
        await authContext.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetProfile), null);
    }

    [HttpPatch("profile/default-address")]
    public async Task<IActionResult> SetDefaultAddress(SetDefaultAddressDto dto, CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null) return Unauthorized();

        var u = await authContext.Users
            .Include(x => x.Profile)
            .ThenInclude(p => p.Addresses!)
            .FirstOrDefaultAsync(x => x.Id == uid.Value, ct);

        if (u is null) return Unauthorized();

        u.Profile ??= new UserProfile { UserId = u.Id };

        if (dto.DefaultShippingAddressId is not null)
        {
            var exists = u.Profile.Addresses.Any(a =>
                a.Id == dto.DefaultShippingAddressId.Value && !a.IsDeleted);

            if (!exists)
            {
                ModelState.AddModelError(nameof(dto.DefaultShippingAddressId), "Address does not exist.");
                return ValidationProblem(ModelState);
            }
        }

        u.Profile.DefaultShippingAddressId = dto.DefaultShippingAddressId;
        await authContext.SaveChangesAsync(ct);
        return NoContent();
    }


    [HttpGet("orders")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyList<MyOrderListItemDto>))]
    public async Task<ActionResult<IReadOnlyList<MyOrderListItemDto>>> GetMyOrders(
    [FromQuery] int skip = 0,
    [FromQuery] int take = 20,
    CancellationToken ct = default)
    {
        var uid = GetUserId();
        if (uid is null) return Unauthorized();

        var orders = await orderService.GetMyOrdersAsync(uid.Value, skip, take, ct);
        return Ok(orders);
    }

    //Helper (parse user id)
    private int? GetUserId()
    {
        if (User?.Identity?.IsAuthenticated != true) return null;

        var idStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        return int.TryParse(idStr, out var parsed) ? parsed : null;
    }
}
