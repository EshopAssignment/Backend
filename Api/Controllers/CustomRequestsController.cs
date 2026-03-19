using Application.DTOs.Order;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomRequestsController(ICustomRequestService customRequestService) : ControllerBase
{
    private readonly ICustomRequestService _customRequestService = customRequestService;

    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreateCustomRequestForm form, CancellationToken ct)
    {
        var result = await _customRequestService.CreateAsync(form, ct);

        if (!result.Ok)
            return BadRequest(new {message = result.Error});

        return Ok(new { message = "Förfrågan mottagen" });
    }
}


