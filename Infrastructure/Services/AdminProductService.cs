using Application.Assemblers;
using Application.DTOs.Admin;
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Services;

public class AdminProductService(
    PallshoppenDbContext dbContext,
    ProductAssembler prodAssembler,
    IDistributedCache cache) : IAdminProductService
{
    private readonly ProductAssembler _assembler = prodAssembler;

    private static readonly OrderStatus[] PopularOrderStatuses =
    [
        OrderStatus.Confirmed,
        OrderStatus.Processing,
        OrderStatus.Shipped,
        OrderStatus.Completed,
        OrderStatus.Refunded
    ];

    public async Task<PagedResult<ProductDto>> GetAllAsync(
        int page,
        int pageSize,
        string? query,
        string? sort,
        List<string>? type,
        List<string>? condition,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isActive,
        CancellationToken ct)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 200 ? 200 : pageSize;

        var term = query?.Trim();
        var s = sort?.Trim().ToLowerInvariant();

        var q = dbContext.Products
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            q = q.Where(p =>
                p.Name.Contains(term) ||
                p.Description.Contains(term) ||
                (p.Sku != null && p.Sku.Contains(term)) ||
                (p.Slug != null && p.Slug.Contains(term)));
        }

        if (type is { Count: > 0 })
        {
            var parsed = type
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(t => ParseEnum<ProductType>(t, nameof(type)))
                .Distinct()
                .ToArray();

            if (parsed.Length > 0)
                q = q.Where(p => parsed.Contains(p.PalletType));
        }

        if (condition is { Count: > 0 })
        {
            var parsed = condition
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(c => ParseEnum<ProductCondition>(c, nameof(condition)))
                .Distinct()
                .ToArray();

            if (parsed.Length > 0)
                q = q.Where(p => parsed.Contains(p.Condition));
        }

        if (minPrice.HasValue)
            q = q.Where(p => p.PriceExVat >= minPrice.Value);

        if (maxPrice.HasValue)
            q = q.Where(p => p.PriceExVat <= maxPrice.Value);

        if (isActive.HasValue)
            q = q.Where(p => p.IsActive == isActive.Value);

        q = ApplySorting(q, s);

        var total = await q.CountAsync(ct);

        var items = await _assembler
            .ProjectToDto(q)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        var name = RequireTrimmed(req.Name, nameof(req.Name), maxLength: 200);
        var desc = SafeTrim(req.Description, maxLength: 1000);

        if (req.PriceExVat < 0)
            throw new ArgumentOutOfRangeException(nameof(req.PriceExVat), "pris must be above 0 ");

        if (req.OnHand < 0)
            throw new ArgumentOutOfRangeException(nameof(req.OnHand), "Stock måste vara 1+");

        if (await dbContext.Products.AnyAsync(p => p.Name == name, ct))
            throw new InvalidOperationException("Product already exist");

        var entity = new Product
        {
            Name = name,
            Description = desc,
            PalletType = ParseEnum<ProductType>(req.PalletType, nameof(req.PalletType)),
            Condition = ParseEnum<ProductCondition>(req.Condition, nameof(req.Condition)),
            PriceExVat = Math.Round(req.PriceExVat, 2),
            VatRate = ParseVatRatePercent(req.VatRatePercent, nameof(req.VatRatePercent)),
            OnHand = req.OnHand,
            Reserved = 0,
            Images = BuildImages(req.Images),
            Sku = "Sku-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
            Slug = Slugify(name),
            IsActive = false
        };

        if (!HasRequiredDate(entity))
            entity.IsActive = false;

        dbContext.Products.Add(entity);

        await dbContext.SaveChangesAsync(ct);
        await InvalidatePublicListsAsync(cache, ct);
        await dbContext.Entry(entity).Collection(p => p.Images).LoadAsync(ct);

        return _assembler.ToDto(entity);
    }

    public async Task<ProductDto> UpdateAsync(int id, AdminUpdateProductRequestDto req, CancellationToken ct) 
    {
        ArgumentNullException.ThrowIfNull(req);

        var name = RequireTrimmed(req.Name, nameof(req.Name), maxLength: 200);
        var desc = SafeTrim(req.Description, maxLength: 1000);

        if (req.PriceExVat < 0)
            throw new ArgumentOutOfRangeException(nameof(req.PriceExVat), "pris must be above 0 ");

        if (req.OnHand < 0)
            throw new ArgumentOutOfRangeException(nameof(req.OnHand), "Stock måste vara 1+");

        var entity = await dbContext.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException($"Product {id} does not exist");

        entity.Name = name;
        entity.Description = desc;
        entity.PalletType = ParseEnum<ProductType>(req.PalletType, nameof(req.PalletType));
        entity.Condition = ParseEnum<ProductCondition>(req.Condition, nameof(req.Condition));
        entity.PriceExVat = Math.Round(req.PriceExVat, 2);
        entity.VatRate = ParseVatRatePercent(req.VatRatePercent, nameof(req.VatRatePercent));
        entity.OnHand = req.OnHand;
        entity.Images.Clear();

        foreach (var img in BuildImages(req.Images))
            entity.Images.Add(img);

        entity.IsActive = HasRequiredDate(entity) && req.IsActive;

        await dbContext.SaveChangesAsync(ct);

        await cache.RemoveAsync($"products:byid:{id}", ct);
        await InvalidatePublicListsAsync(cache, ct);

        return _assembler.ToDto(entity);
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct)
    {
        var entity = await dbContext.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (entity is null) return false;

        if (isActive && !HasRequiredDate(entity))
        {
            entity.IsActive = false;
            await dbContext.SaveChangesAsync(ct);
            return true;
        }

        if (entity.IsActive == isActive) return true;

        entity.IsActive = isActive;
        await dbContext.SaveChangesAsync(ct);

        await cache.RemoveAsync($"products:byid:{id}", ct);
        await InvalidatePublicListsAsync(cache, ct);
        return true;
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var p = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return p is null ? null : _assembler.ToDto(p);
    }

    private IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sort)
    {
        return sort switch
        {
            "price_asc" => query.OrderBy(p => p.PriceExVat),
            "price_desc" => query.OrderByDescending(p => p.PriceExVat),
            "name_asc" => query.OrderBy(p => EF.Functions.Collate(p.Name, "Latin1_General_100_BIN2")),
            "name_desc" => query.OrderByDescending(p => EF.Functions.Collate(p.Name, "Latin1_General_100_BIN2")),
            "popular" => ApplyPopularSorting(query),
            _ => query.OrderBy(p => p.Id),
        };
    }

    private IQueryable<Product> ApplyPopularSorting(IQueryable<Product> products)
    {
        var salesByProduct = dbContext.OrderItems
            .AsNoTracking()
            .Where(i => PopularOrderStatuses.Contains(i.Order.OrderStatus))
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                UnitsSold = g.Sum(x => x.Quantity)
            });

        return products
            .GroupJoin(
                salesByProduct,
                product => product.Id,
                sales => sales.ProductId,
                (product, sales) => new
                {
                    Product = product,
                    UnitsSold = sales.Select(x => (int?)x.UnitsSold).FirstOrDefault() ?? 0
                })
            .OrderByDescending(x => x.UnitsSold)
            .ThenBy(x => EF.Functions.Collate(x.Product.Name, "Latin1_General_100_BIN2"))
            .Select(x => x.Product);
    }

    // helpers
    private static TEnum ParseEnum<TEnum>(string? value, string param) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("värde saknas", param);

        if (Enum.TryParse<TEnum>(value, true, out var ok))
            return ok;

        throw new ArgumentException($"Ogiltigt värde '{value}' för {typeof(TEnum).Name}", param);
    }

    private static VatRate ParseVatRatePercent(int vatRatePercent, string paramName)
        => vatRatePercent switch
        {
            6 => VatRate.Vat6,
            12 => VatRate.Vat12,
            25 => VatRate.Vat25,
            _ => throw new ArgumentOutOfRangeException(paramName, vatRatePercent, "VatRatePercent måste vara 6, 12 eller 25.")
        };

    private static string Slugify(string input) =>
        input.Trim().ToLowerInvariant().Replace("(", "").Replace(")", "").Replace("  ", " ").Replace(' ', '-');

    private static string RequireTrimmed(string? value, string paramName, int maxLength = 0)
    {
        var v = value?.Trim();

        if (string.IsNullOrWhiteSpace(v))
            throw new ArgumentException($"{paramName} is required", paramName);

        if (maxLength > 0 && v.Length > maxLength)
            throw new ArgumentException(paramName, $"MaxLenght is {maxLength} charachters");

        return v;
    }

    private static string SafeTrim(string? value, int maxLength = 0)
    {
        var v = value?.Trim() ?? string.Empty;
        if (maxLength > 0 && v.Length > maxLength)
            return v[..maxLength];
        return v;
    }

    private static bool HasRequiredDate(Product p)
    {
        if (string.IsNullOrWhiteSpace(p.Name)) return false;
        if (string.IsNullOrWhiteSpace(p.Description)) return false;
        if (p.PriceExVat < 0) return false;

        if (!Enum.IsDefined(typeof(ProductType), p.PalletType)) return false;
        if (!Enum.IsDefined(typeof(ProductCondition), p.Condition)) return false;

        var vat = (int)p.VatRate;
        if (vat is not (6 or 12 or 25)) return false;

        if (p.Images is null || p.Images.Count == 0) return false;
        if (p.Images.Count(i => i.IsPrimary) != 1) return false;

        return true;
    }

    private static Task BumpAsync(IDistributedCache cache, string key, CancellationToken ct)
    {
        return cache.SetStringAsync(key, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(), ct);
    }

    private static Task InvalidatePublicListsAsync(IDistributedCache cache, CancellationToken ct)
        => Task.WhenAll(
            BumpAsync(cache, "products:ver:list", ct),
            BumpAsync(cache, "products:ver:suggest", ct)
        );

    private static List<ProductImage> BuildImages(IEnumerable<AdminProductImageRequestDto>? reqImages)
    {
        var imgs = (reqImages ?? Enumerable.Empty<AdminProductImageRequestDto>())
            .Where(x => !string.IsNullOrEmpty(x.CardUrl))
            .Select(x => new ProductImage
            {
                OriginalUrl = x.OriginalUrl.Trim(),
                LargeUrl = x.LargeUrl.Trim(),
                CardUrl = x.CardUrl.Trim(),
                StackUrl = x.StackUrl.Trim(),
                ThumbUrl = x.ThumbUrl.Trim(),
                SortOrder = x.SortOrder,
                IsPrimary = x.IsPrimary,
                AltText = string.IsNullOrWhiteSpace(x.AltText) ? null : x.AltText.Trim()
            })
            .OrderBy(x => x.SortOrder)
            .ToList();

        for (var i = 0; i < imgs.Count; i++)
            imgs[i].SortOrder = i + 1;

        if (imgs.Count > 0)
        {
            var firstPrimary = imgs.FirstOrDefault(i => i.IsPrimary) ?? imgs[0];
            foreach (var img in imgs) img.IsPrimary = ReferenceEquals(img, firstPrimary);
            firstPrimary.IsPrimary = true;
        }

        return imgs;
    }
}