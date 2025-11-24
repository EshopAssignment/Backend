using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDto>))]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 4,
        [FromQuery] string? query = null,
        [FromQuery] string? sort = null,
        [FromQuery] List<string>? type = null,
        [FromQuery] List<string>? condition = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,

        CancellationToken cancellationtoken = default)
    {
        var products = await productService.GetAllPagedAsync(
            page, pageSize, query, sort, type, condition, minPrice, maxPrice, cancellationtoken);
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBytId(int id, CancellationToken cancellationtoken)
    {
        var product = await productService.GetByIdAsync(id, cancellationtoken);

        if (product is null)
            return NotFound();

        return Ok(product);
    }
}
