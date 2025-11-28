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
    public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count == 0)
            return BadRequest("Order must contain atleast one item");
        if (request.Items.Any(i => i.Quantity <= 0))
            return BadRequest("item quantity must be >= 1");

        try
        {
            var result = await orderService.CreateOrderAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.OrderId },
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

        var dto = await orderService.GetOrderByIdAsync(id, cancellationToken);
            if (dto is null) return NotFound();

        return Ok(dto);
    }
}
