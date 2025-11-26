using System.Runtime.InteropServices;
using Application.DTOs.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/admin/products")]
public class AdminProductController(IProductService productService, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok (await productService.GetAllAsync(page, pageSize, null, null, null, null, null, null, ct));
    
    
    [HttpGet("{id:int}", Name = "GetProductById_AdminEcho")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var dto = await productService.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminCreateProductRequestDto req, CancellationToken ct = default)
    {
        var dto = await productService.CreateAsync(req, ct);
        return CreatedAtRoute("GetProductById_AdminEcho", new { id = dto.Id }, dto);
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateProductRequestDto req, CancellationToken ct = default)
    {
        var updated = await productService.UpdateAsync(id, req, ct);
        return updated is null ? NotFound() : Ok(updated); ;
    }
    [HttpPut("{id:int}/image")]
    public async Task<IActionResult> UploadImage(int id, IFormFile file, CancellationToken ct)
    {
        var prod = await productService.GetByIdAsync(id, ct);
        if (prod is null)return NotFound();

        if (file is null || file.Length == 0) return BadRequest("Must upload file");

        var dir = Path.Combine(env.WebRootPath ?? "wwwroot", "images", "products");
        Directory.CreateDirectory(dir);

        var ext = Path.GetExtension(file.FileName);
        var name = $"product_{id}_{DateTime.UtcNow.Ticks}{ext}";
        var path = Path.Combine(dir, name);
        await using (var fs = System.IO.File.Create(path))
            await file.CopyToAsync(fs, ct);

        var url = $"/images/products/{name}";
        await productService.SetImageUrlAsync(id, url, ct);

        return Ok(new { imgUrl = url });
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> ToggleActive(int id, [FromBody] bool IsActive, CancellationToken ct = default)
    {
        var ok = await productService.SetActiveAsync(id, IsActive, ct);
        return ok? NoContent() : NotFound();
    }



}
