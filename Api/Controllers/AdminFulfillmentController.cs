using Application.DTOs;
using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]

public class AdminFulfillmentController(IAdminFulfillmentService fulfillmentService) : ControllerBase
{
    [HttpGet("queue")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResultDto<AdminFulfillmentOrderDto>))]
    public async Task<IActionResult> GetQueue(
        [FromQuery] FulfillmentStatus? fulfillmentStatus = null,
        [FromQuery] bool overdueOnly = false,
        [FromQuery] string? query = null,
        [FromQuery] int page = -1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var dto = await fulfillmentService.GetQueueAsync(
            new FulfillmentQueueFilterDto(
                fulfillmentStatus,
                overdueOnly,
                query,
                page,
                pageSize
                ), ct);
        return Ok(dto);
    }

    [HttpGet("{orderId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminFulfillmentOrderDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int orderId, CancellationToken ct = default)
    {
        var dto = await fulfillmentService.GetByIdAsync(orderId, ct);

        if (dto is null)
            return NotFound();

        return Ok(dto);
    }

    [HttpPost("{orderId:int}/mark-fulfilled")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkFulfilled(
        [FromRoute] int orderId,
        [FromBody] MarkOrderFulfillmentRequest request,
        CancellationToken ct = default)
    {
        await fulfillmentService.MarkFulfilledAsync(orderId, request.Note, ct);
        return NoContent();
    }
    [HttpPost("{orderId:int}/reopen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reopen(
        [FromRoute] int orderId,
        [FromBody] ReopenFulfillmentRequest request,
        CancellationToken ct = default)
    {
        await fulfillmentService.ReopenAsync(orderId, request.Note, ct);
        return NoContent();
    }

    [HttpPut("{orderId:int}/note")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetFulfillmentNote(
    [FromRoute] int orderId,
    [FromBody] SetFulfillmentNoteRequest request,
    CancellationToken ct = default)
    {
        await fulfillmentService.SetFulfillmentNoteAsync(orderId, request.Note, ct);
        return NoContent();
    }
}
