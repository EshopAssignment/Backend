using Application.DTOs.Admin;
using Application.DTOs.Product;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/admin/custom-request")]
[ApiController]
public class AdminCustomRequestsController(IAdminCustomRequestService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AdminCustomRequestListItemDto>))]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? query = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var res = await service.GetAllAsync(page, pageSize, query, status, ct);
        return Ok(res);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AdminCustomRequestDetailsDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var res = await service.GetByIdAsync(id, ct);
        return res is null ? NotFound() : Ok(res);
    }

    [HttpPost("{id:int}/quotes")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AdminCustomQuoteDetailsDto))]
    public async Task<IActionResult> CreateQuote(int id, [FromBody] AdminCreateCustomQuoteDto body, CancellationToken ct = default)
    {
        var res = await service.CreateQuoteAsync(id, body, ct);
        return Ok(res);
    }
}
