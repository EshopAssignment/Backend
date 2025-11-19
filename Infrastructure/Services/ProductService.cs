
using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

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
