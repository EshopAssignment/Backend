using Application.DTOs.Shipping;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShippingController(IOrderService orders, IPostNordClient postnord) : ControllerBase
{
    [HttpGet("service-points")]
    public async Task<ActionResult<IReadOnlyList<ServicePointDto>>> GetServicePoints(
        [FromQuery] string postalCode,
        [FromQuery] string? city,
        CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(postalCode)) return BadRequest("postalCode is required");
        var list = await postnord.FindServicePointsAsync(postalCode, city, ct);
        return Ok(list);
    }

    [HttpPut("orders/{orderNumber}/selection")]
    public async Task<IActionResult> SetSelection(string orderNumber, [FromBody] SetShippingSelectionDto body, CancellationToken ct)
    {
        try
        {
            var ok = await orders.SetShippingSelectionAsync(orderNumber, body, ct);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
