using System;
using System.Collections.Generic;
using System.Text;
using Application.DTOs.Admin;
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AdminProductService(PallshoppenDbContext dbContext) : IAdminProductService
{
    //helpers
    private static TEnum ParseEnum<TEnum>(string? value, string param) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("värde saknas", param);
        if (Enum.TryParse<TEnum>(value, true, out var ok)) return ok;
        throw new ArgumentException($"Ogiltigt värde '{value}' för {typeof(TEnum).Name}", param);
    }
    private static string Slugify(string input) =>
        input.Trim().ToLowerInvariant().Replace("(", "").Replace(")", "").Replace("  ", " ").Replace(' ', '-');
    private static ProductDto ToDto(Product p) =>
        new(p.Id,
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
                p.Slug);
    public async Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize, string? query, string? sort,List<string>? type, List<string>? condition,decimal? minPrice, decimal? maxPrice, bool? isActive, CancellationToken ct)
    {
        var q = dbContext.Products.AsNoTracking().AsQueryable();

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
        if (isActive.HasValue) q = q.Where(p => p.IsActive == isActive.Value);

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
    public async Task<ProductDto> CreateAsync(AdminCreateProductRequestDto req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);
        var name = req.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException("Name is Required", nameof(name));
        }

        if (req.PriceExVat < 0) throw new ArgumentOutOfRangeException(nameof(req.PriceExVat), "pris must be above 0 ");
        if (req.OnHand < 0) throw new ArgumentOutOfRangeException(nameof(req.OnHand), "Stock måste vara 1+");

        if (await dbContext.Products.AnyAsync(p => p.Name == name, ct))
            throw new InvalidOperationException("Product already exist");

        var entity = new Product
        {
            Name = name,
            Description = req.Description.Trim() ?? string.Empty,
            PalletType = ParseEnum<ProductType>(req.PalletType, nameof(req.PalletType)),
            Condition = ParseEnum<ProductCondition>(req.Condition, nameof(req.Condition)),
            PriceExVat = Math.Round(req.PriceExVat, 2),
            OnHand = req.OnHand,
            Reserved = 0,
            ImgUrl = string.Empty,
            IsActive = req.IsActive,
            Sku = "Sku-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            Slug = Slugify(name)
        };

        dbContext.Products.Add(entity);
        await dbContext.SaveChangesAsync();
        return ToDto(entity);

    }
    public async Task<ProductDto> UpdateAsync(int id, AdminUpdateProductRequestDto req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);
        var name = req.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Namn is required", nameof(req));
        if (req.PriceExVat < 0) throw new ArgumentOutOfRangeException(nameof(req.PriceExVat), "pris must be above 0 ");
        if (req.OnHand < 0) throw new ArgumentOutOfRangeException(nameof(req.OnHand), "Stock måste vara 1+");

        var entity = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException($"Produkct {id} does not exist");

        entity.Name = name;
        entity.Description = req.Description.Trim() ?? string.Empty;
        entity.PalletType = ParseEnum<ProductType>(req.PalletType, nameof(req.PalletType));
        entity.Condition = ParseEnum<ProductCondition>(req.Condition, nameof(req.Condition));
        entity.PriceExVat = Math.Round(req.PriceExVat, 2);
        entity.OnHand = req.OnHand;
        entity.Reserved = 0;
        entity.ImgUrl = string.Empty;
        entity.IsActive = req.IsActive;


        await dbContext.SaveChangesAsync(ct);
        return ToDto(entity);
    }
    public async Task<bool> SetActiveAsync(int id, bool IsActive, CancellationToken ct)
    {
        var entity = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null) return false;
        if (entity.IsActive == IsActive) return true;
        entity.IsActive = IsActive;
        await dbContext.SaveChangesAsync(ct);
        return true;
    }
    public async Task SetImageUrlAsync(int id, string imgUrl, CancellationToken ct)
    {
        var url = imgUrl?.Trim() ?? string.Empty;
        var entity = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException($" produkt {id} finns inte");

        entity.ImgUrl = url;
        await dbContext.SaveChangesAsync(ct);
    }
    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var p = await dbContext.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return p is null ? null : ToDto(p);
    }
}
