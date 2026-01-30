using Application.DTOs.Reservations;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ReservationController(IInventoryService inventory) : ControllerBase
{
    [HttpPut("api/cart/reservations")]
    public async Task<IActionResult> SetReservation([FromBody] SetCartReservationDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.CartId)) return BadRequest(new { error = "CART_ID_REQUIRED" });
        if (dto.ProductId <= 0) return BadRequest(new { error = "PRODUCT_ID_INVALID" });
        if (dto.Quantity < 0) return BadRequest(new { error = "QTY_INVALID" });

        var ttl = TimeSpan.FromMinutes(dto.ReservationTtlMinutes <= 0 ? 30 : dto.ReservationTtlMinutes);

        var (ok, err) = await inventory.SetReservationQtyAsync(dto.ProductId, dto.Quantity, dto.CartId, ttl, ct);
        return ? Ok() : BadRequest(new { error = err });

    }
}
