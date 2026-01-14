using System.Security.Claims;
using Application.DTOs.Order;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderCreatedDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto request,  CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count == 0)
            return BadRequest("Order must contain atleast one item");

        if (request.Items.Any(i => i.Quantity <= 0))
            return BadRequest("item quantity must be >= 1");
        
        if (string.IsNullOrWhiteSpace(request.CartId))
            return BadRequest("CartId is required");



        int? userId = null;
        if (User?.Identity?.IsAuthenticated == true)
        {
            var idStr = 
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub");

            if (int.TryParse(idStr, out var parsed))
                userId = parsed;
        }

        try
        {
            var result = await orderService.CreateAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetByNumber),
                new { orderNumber = result.OrderNumber},
                result

            );
        }

        catch(InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
       
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderCreatedDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0) return NotFound();

        var dto = await orderService.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("by-number/{orderNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderCreatedDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNumber(string orderNumber, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(orderNumber)) return NotFound();
        var dto = await orderService.GetByNumberAsync(orderNumber, ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}
