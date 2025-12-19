
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ProductService(PallshoppenDbContext dbContext) : IProductService
{
    private static TEnum ParseEnum<TEnum>(string? value, string paramName) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("no value", paramName);
        if (Enum.TryParse<TEnum>(value, true, out var parsed)) return parsed;
        throw new ArgumentException($"Inavlid value '{value}' for {typeof(TEnum).Name}", paramName);
    }
    private static ProductDto ToDto(Product p) =>
        new(
        p.Id,
        p.Name,
        p.Description,
        p.ImgUrl,
        p.PriceExVat,
        p.PalletType.ToString(),
        p.Condition.ToString(),
        p.StockStatus.ToString(),
        p.OnHand,
        p.Reserved,
        p.Available,
        p.IsActive,
        p.Sku,
        p.Slug
        );


    public async Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize, string? query, string? sort, List<string>? type, List<string>? condition, decimal? minPrice, decimal? maxPrice, bool? inStock, CancellationToken ct)
    {
        var q = dbContext.Products.AsNoTracking().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(p =>
                p.Name.Contains(term) ||
                p.Description.Contains(term) ||
                (p.Sku != null && p.Sku.Contains(term)) ||
                (p.Slug != null && p.Slug.Contains(term)));
        }

        if (type is { Count: > 0 })
        {
            var parsed = type.Select(t => ParseEnum<ProductType>(t, nameof(type))).Distinct().ToArray();
            q = q.Where(p => parsed.Contains(p.PalletType));
        }

        if (condition is { Count: > 0 })
        {
            var parsed = condition.Select(c => ParseEnum<ProductCondition>(c, nameof(condition))).Distinct().ToArray();
            q = q.Where(p => parsed.Contains(p.Condition));
        }

        if (minPrice.HasValue) q = q.Where(p => p.PriceExVat >= minPrice.Value);
        if (maxPrice.HasValue) q = q.Where(p => p.PriceExVat <= maxPrice.Value);

        if (inStock == true)
        {
            q = q.Where(p => (p.OnHand - p.Reserved) > 0);
        }

        q = sort switch
        {
            "price_asc" => q.OrderBy(p => p.PriceExVat),
            "price_desc" => q.OrderByDescending(p => p.PriceExVat),
            "name_asc" => q.OrderBy(p => p.Name),
            "name_desc" => q.OrderByDescending(p => p.Name),
            _ => q.OrderBy(p => p.Id),
        };

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.ImgUrl,
                p.PriceExVat,
                p.PalletType.ToString(),
                p.Condition.ToString(),
                p.StockStatus.ToString(),
                p.OnHand,
                p.Reserved,
                p.Available,
                p.IsActive,
                p.Sku,
                p.Slug))
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
        var p = await dbContext.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IsActive && p.Id == id, cancellationToken);

        return p is null ? null : ToDto(p);
    }
    public async Task<IEnumerable<ProductSuggestionDto>> SuggestionAsync(string q, int take, CancellationToken ct)
    {
        var term = q.Trim();
        var size = Math.Clamp(take, 1, 20);

        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive &&
                (EF.Functions.Like(p.Name, $"%{term}%") ||
                 (p.Sku != null && EF.Functions.Like(p.Sku, $"%{term}%")) ||
                 (p.Slug != null && EF.Functions.Like(p.Slug, $"%{term}%"))))
            .OrderBy(p => p.Name)
            .Take(size)
            .Select(p => new ProductSuggestionDto(
            p.Id,
            p.Name,
            p.PriceExVat,
            p.ImgUrl,
            p.Slug ?? "",
            p.Sku ?? ""
            ))
            .ToListAsync(ct);
    }

}

