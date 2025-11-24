
using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ProductService(PallshoppenDbContext dbContext) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Id)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.PalletType,
                p.Condition,
                p.Price,
                p.StockQuantity,
                p.ImgUrl,
                p.IsActive

                ))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<ProductDto>> GetAllPagedAsync(
        int page, int pageSize, string? query, string? sort, List<string>? type, List<string>? condition, decimal? minPrice, decimal? maxPrice, CancellationToken ct)
    {
        var q = dbContext.Products.AsQueryable().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(p => p.Name.Contains(term) || p.Description.Contains(term) || p.PalletType.Contains(term) || p.Condition.Contains(term));
        }

        if (type is { Count : > 0})
            q = q.Where(p => type.Contains(p.PalletType));

        if (condition is { Count: > 0 })
            q = q.Where(p => condition.Contains(p.Condition));

        if (minPrice.HasValue)
            q = q.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            q = q.Where(p => p.Price <= maxPrice.Value);

        q = sort switch
        {
            "price_asc" => q.OrderBy(p => p.Price),
            "price_desc" => q.OrderByDescending(p => p.Price),
            "name_asc" => q.OrderBy(p => p.Name),
            "name_desc" => q.OrderByDescending(p => p.Name),
            _ => q.OrderBy(p => p.Id),
        };

        var total = await q.CountAsync(ct);
        var item = await q.Skip((page -1) * pageSize).Take(pageSize)
           .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.PalletType,
                p.Condition,
                p.Price,
                p.StockQuantity,
                p.ImgUrl,
                p.IsActive
               )).ToListAsync(ct);

        return new PagedResult<ProductDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Items = item
        };
    }
    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var p = await dbContext.Products
            .Where(p => p.IsActive && p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (p is null)
            return null;

        return new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.PalletType,
                p.Condition,
                p.Price,
                p.StockQuantity,
                p.ImgUrl,
                p.IsActive
            );
    }

}
