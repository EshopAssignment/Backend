using Application.Assemblers;
using Application.DTOs.Admin;
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AdminProductService(PallshoppenDbContext dbContext, ProductAssembler prodAssembler) : IAdminProductService
{
    private readonly ProductAssembler _assembler = prodAssembler;

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
            (int)p.VatRate,

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

        var name = RequireTrimmed(req.Name, nameof(req.Name), maxLength:200);
        var desc = SafeTrim(req.Description, maxLength:1000);


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

            ImgUrl = SafeTrim(req.ImgUrl, maxLength: 500), 
            Sku = "Sku-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
            Slug = Slugify(name),

            IsActive = false
        };

        if(!HasRequiredDate(entity))
            entity.IsActive = false;

        dbContext.Products.Add(entity);
        await dbContext.SaveChangesAsync(ct);

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

        var entity = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException($"Produkct {id} does not exist");

        entity.Name = name;
        entity.Description = desc;
        entity.PalletType = ParseEnum<ProductType>(req.PalletType, nameof(req.PalletType));
        entity.Condition = ParseEnum<ProductCondition>(req.Condition, nameof(req.Condition));
        entity.PriceExVat = Math.Round(req.PriceExVat, 2);
        entity.VatRate = ParseVatRatePercent(req.VatRatePercent, nameof(req.VatRatePercent));
        entity.OnHand = req.OnHand;

        if (!string.IsNullOrWhiteSpace(req.ImgUrl))
            entity.ImgUrl = req.ImgUrl.Trim();

        entity.IsActive = HasRequiredDate(entity) ? req.IsActive : false;


        await dbContext.SaveChangesAsync(ct);
        return _assembler.ToDto(entity);
    }
    public async Task<bool> SetActiveAsync(int id, bool IsActive, CancellationToken ct)
    {
        var entity = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if( entity is null) return false;

        if (IsActive && !HasRequiredDate(entity))
        {
            entity.IsActive = false;
            await dbContext.SaveChangesAsync(ct);
            return true;
        }

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
    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var p = await dbContext.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return p is null ? null : _assembler.ToDto(p);
    }
    //helpers
    private static TEnum ParseEnum<TEnum>(string? value, string param) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("värde saknas", param);
        if (Enum.TryParse<TEnum>(value, true, out var ok)) return ok;
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
            throw new ArgumentException(paramName,$"MaxLenght is {maxLength} charachters");

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

        return true;
    }
}
