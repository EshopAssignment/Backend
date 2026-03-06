using System.ComponentModel;
using System.Reflection;
using Application.DTOs.Admin;
using Application.DTOs.Options;
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/admin/products")]
public class AdminProductController(IAdminProductService productService) : ControllerBase
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
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var result = await productService.GetAllAsync(
            page, pageSize, query, sort, type, condition, minPrice, maxPrice, isActive, ct);

        return Ok(result);
    }
    [HttpGet("{id:int}", Name = "GetProductById_Admin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var dto = await productService.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProductDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AdminCreateProductRequestDto req, CancellationToken ct = default)
    {

        var dto = await productService.CreateAsync(req, ct);
        return CreatedAtRoute("GetProductById_Admin", new { id = dto.Id }, dto);
    }
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateProductRequestDto req, CancellationToken ct = default)
    {
        if (id != req.Id) return BadRequest("Id missmatch");
        var updated = await productService.UpdateAsync(id, req, ct);
        return Ok(updated);
    }
    [HttpPatch("{id:int}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleActive(int id,[FromBody] ToggleActiveRequest? body,[FromQuery] bool? isActive,CancellationToken ct = default) 
    {
        var value = body?.IsActive ?? isActive;
        if (value is null)
            return BadRequest("sum ting wong");

        var ok = await productService.SetActiveAsync(id, value.Value, ct);
        return ok ? NoContent() : NotFound();
    }
    [HttpGet("options")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminProductOptionsDto))]
    public IActionResult GetOptions()
    {
        var dto = new AdminProductOptionsDto(
            GetEnumOptions<ProductType>(),
            GetEnumOptions<ProductCondition>(),
            new[]
            {
            new EnumOptionDto("6", "6%", 6),
            new EnumOptionDto("12", "12%", 12),
            new EnumOptionDto("25", "25%", 25),
            }
        );

        return Ok(dto);
    }

    private static IEnumerable<EnumOptionDto> GetEnumOptions<T>() where T : struct, Enum
    {
        foreach (var v in Enum.GetValues<T>())
        {
            var name = v.ToString();
            var label = typeof(T).GetMember(name)
                .First()
                .GetCustomAttribute<DescriptionAttribute>()?.Description ?? name;

            yield return new EnumOptionDto(
                name,
                label,
                Convert.ToInt32(v)
            );
        }
    }
}

