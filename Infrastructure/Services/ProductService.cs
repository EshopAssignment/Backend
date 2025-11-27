
using Application.DTOs;
using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ProductService(PallshoppenDbContext dbContext) : IProductService
{
    public async Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize, string? query, string? sort, List<string>? type, List<string>? condition, decimal? minPrice, decimal? maxPrice, CancellationToken ct)
        
    {
        var q = dbContext.Products.AsNoTracking().Where(p => p.IsActive);

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
    public async Task<PagedResult<ProductDto>> GetAllAdminAsync(int page, int pageSize, string? query, string? sort,List<string>? type, List<string>? condition, decimal? minPrice, decimal? maxPrice, bool? isActive, CancellationToken ct)
    {
        var q = dbContext.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(p =>
                p.Name.Contains(term) ||
                p.Description.Contains(term) ||
                p.PalletType.Contains(term) ||
                p.Condition.Contains(term));
        }

        if (type is { Count: > 0 })
            q = q.Where(p => type.Contains(p.PalletType));

        if (condition is { Count: > 0 })
            q = q.Where(p => condition.Contains(p.Condition));

        if (minPrice.HasValue) q = q.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) q = q.Where(p => p.Price <= maxPrice.Value);

        if (isActive.HasValue) q = q.Where(p => p.IsActive == isActive.Value);

        q = sort switch
        {
            "price_asc" => q.OrderBy(p => p.Price),
            "price_desc" => q.OrderByDescending(p => p.Price),
            "name_asc" => q.OrderBy(p => p.Name),
            "name_desc" => q.OrderByDescending(p => p.Name),
            _ => q.OrderBy(p => p.Id),
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto(
                p.Id, p.Name, p.Description, p.PalletType, p.Condition,
                p.Price, p.StockQuantity, p.ImgUrl, p.IsActive))
            .ToListAsync(ct);

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
    public async Task<ProductDto> CreateAsync(AdminCreateProductRequestDto req, CancellationToken ct)
    {
        if (req is null)
        {
            throw new ArgumentNullException(nameof(req));
        }

        var name = req.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Namn är måste", nameof(req));

        if (req.Price < 0)
            throw new ArgumentOutOfRangeException(nameof(req.Price), "priset måste vara 1+");

        if (req.StockQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(req.StockQuantity), "Stock måste vara 1+");

        var exists = await dbContext.Products.AnyAsync(p => p.Name == name, ct);
        if (exists)
            throw new InvalidOperationException("Produkt finns redan");

        var entity = new Product
        {
            Name = name,
            Description = req.Description?.Trim() ?? string.Empty,
            PalletType = req.PalletType?.Trim() ?? string.Empty,
            Condition = req.Condition?.Trim() ?? string.Empty,
            Price = Math.Round(req.Price, 2),
            StockQuantity = req.StockQuantity,
            ImgUrl = string.Empty,
            IsActive = req.IsActive
        };

        dbContext.Products.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        return ToDto(entity);
    }
    public async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct)
    {
        var entity = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null) return false;

        if (entity.IsActive == isActive) return true;

        entity.IsActive = isActive;
        await dbContext.SaveChangesAsync(ct);
        return true;
    }
    public async Task SetImageUrlAsync(int id, string imgUrl, CancellationToken ct)
    {
        var url = imgUrl?.Trim() ?? string.Empty;

        var entity = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null)
            throw new KeyNotFoundException($" produkt {id} finns inte");

        entity.ImgUrl = url;
        await dbContext.SaveChangesAsync(ct);
    }
    public async Task<ProductDto> UpdateAsync(int id, AdminUpdateProductRequestDto req, CancellationToken ct)
    {
        if (req is null)
        {
            throw new ArgumentNullException(nameof(req));
        }

        var name = req.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Namn är måste", nameof(req));

        if (req.Price < 0)
            throw new ArgumentOutOfRangeException(nameof(req.Price), "priset måste vara 1+");

        if (req.StockQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(req.StockQuantity), "Stock måste vara 1+");

        var entity = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Produkt {id} finns inte");

        entity.Name = name;
        entity.Description = req.Description?.Trim() ?? string.Empty;
        entity.PalletType = req.PalletType?.Trim() ?? string.Empty;
        entity.Condition = req.Condition?.Trim() ?? string.Empty;
        entity.Price = Math.Round(req.Price, 2);
        entity.StockQuantity = req.StockQuantity;
        entity.IsActive = req.IsActive;

        await dbContext.SaveChangesAsync(ct);

        return ToDto(entity);

    }
    private static ProductDto ToDto(Product p) =>
        new ProductDto(            
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

