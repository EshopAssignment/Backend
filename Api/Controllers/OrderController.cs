using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderCreatedDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count == 0)
            return BadRequest("Order must contain atleast one item");

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
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        //not implemeted
        return NotFound();
    }
}
