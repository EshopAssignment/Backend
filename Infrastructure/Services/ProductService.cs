
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

    public async Task<PagedResult<ProductDto>> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Id)
            .AsQueryable();

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page -1) * pageSize)
            .Take(pageSize)
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

        return new PagedResult<ProductDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
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
