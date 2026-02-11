using Application.DTOs.Product;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<ProductDto>))]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? query = null,
        [FromQuery] string? sort = null,
        [FromQuery] List<string>? type = null,
        [FromQuery] List<string>? condition = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? inStock = null,
        CancellationToken cancellationtoken = default)
    {

        try
        { 

        var products = await productService.GetAllAsync(
            page, pageSize, query, sort, type, condition, minPrice, maxPrice, inStock, cancellationtoken);

        return Ok(products);
        
        }
        catch(ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationtoken)
    {
        var product = await productService.GetByIdAsync(id, cancellationtoken);

        if (product is null)
            return NotFound();

        return Ok(product);
    }

    [HttpGet("suggest")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductSuggestionDto>))]
    public async Task<IActionResult> Suggestion([FromQuery] string q, [FromQuery] int take = 8, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<ProductSuggestionDto>());

        var result = await productService.SuggestionAsync(q, take, ct);
        return Ok(result);
    }
}
