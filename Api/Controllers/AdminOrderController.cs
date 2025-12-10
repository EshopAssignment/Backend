using Application.DTOs.Admin;
using Application.DTOs.Product;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
public class AdminOrderController(IAdminOrderService adminOrderService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AdminOrderListItemDto>))]
    public async Task<IActionResult> ListAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? query = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null, 
        CancellationToken ct = default)

    {
        var res = await adminOrderService.GetAllAsync(page, pageSize, query, status, from, to, ct);
        return Ok(res);
    }


    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AdminOrderDetailsDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminOrderDetailsDto>> GetById(int id, CancellationToken ct = default)
    {
        var res = await adminOrderService.GetByIdAsync(id, ct);
        return res is null ? NotFound() : Ok(res);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AdminUpdateOrderStatusRequest body, CancellationToken ct = default)
    {

        var ok = await adminOrderService.UpdateStatusAsync(id, body.OrderStatus.ToString(), ct);
        return ok ? NoContent() : NotFound();
    }

}
