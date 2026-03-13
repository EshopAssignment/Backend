
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Assemblers;
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ProductService(
    PallshoppenDbContext dbContext,
    ProductAssembler assembler,
    IDistributedCache cache,
    ILogger<ProductService> logger,
    IConfiguration config) : IProductService
{
    private static TEnum ParseEnum<TEnum>(string? value, string paramName) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("no value", paramName);
        if (Enum.TryParse<TEnum>(value, true, out var parsed)) return parsed;
        throw new ArgumentException($"Inavlid value '{value}' for {typeof(TEnum).Name}", paramName);
    }
    private readonly ProductAssembler _assembler = assembler;
    private readonly bool _cacheEnabled = config.GetValue("Cache:Enabled", true); // flag to enable/disable caching

    private static readonly JsonSerializerOptions CacheJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private static string MakeKey(string prefix, string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return $"{prefix}:{Convert.ToHexString(bytes)}";
    }


    public async Task<PagedResult<ProductDto>> GetAllAsync(
        int page, int pageSize, string? query, string? sort,
        List<string>? type, List<string>? condition,
        decimal? minPrice, decimal? maxPrice, bool? inStock,
        CancellationToken ct)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 200 ? 200 : pageSize;

        var qTerm = query?.Trim();
        var s = sort?.Trim();

        var typeNorm = type?.Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(x => x.Trim())
                            .OrderBy(x => x)
                            .ToArray() ?? [];

        var condNorm = condition?.Where(x => !string.IsNullOrWhiteSpace(x))
                                 .Select(x => x.Trim())
                                 .OrderBy(x => x)
                                 .ToArray() ?? [];

        var verList = _cacheEnabled
            ? (await cache.GetStringAsync("products:ver:list", ct) ?? "0")
            : "0";



        var rawKey =
           $"ver={verList}&page={page}&pageSize={pageSize}&query={qTerm}&sort={s}&" +
           $"type={string.Join(",", typeNorm)}&condition={string.Join(",", condNorm)}&" +
           $"min={minPrice?.ToString() ?? ""}&max={maxPrice?.ToString() ?? ""}&inStock={inStock?.ToString() ?? ""}";


        var cacheKey = MakeKey("products:list", rawKey);

        if (_cacheEnabled)
        {
            var cached = await cache.GetStringAsync(cacheKey, ct);
            if (cached is not null)
            {
                logger.LogInformation("Redis HIT {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<PagedResult<ProductDto>>(cached, CacheJson)!;
            }

            logger.LogInformation("Redis MISS {CacheKey}", cacheKey);
        }

        var q = dbContext.Products.AsNoTracking().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(qTerm))
        {
            q = q.Where(p =>
                p.Name.Contains(qTerm) ||
                p.Description.Contains(qTerm) ||
                (p.Sku != null && p.Sku.Contains(qTerm)) ||
                (p.Slug != null && p.Slug.Contains(qTerm)));
        }

        if (typeNorm.Length > 0)
        {
            var parsed = typeNorm.Select(t => ParseEnum<ProductType>(t, nameof(type)))
                                 .Distinct()
                                 .ToArray();
            q = q.Where(p => parsed.Contains(p.PalletType));
        }

        if (condNorm.Length > 0)
        {
            var parsed = condNorm.Select(c => ParseEnum<ProductCondition>(c, nameof(condition)))
                                 .Distinct()
                                 .ToArray();
            q = q.Where(p => parsed.Contains(p.Condition));
        }

        if (minPrice.HasValue) q = q.Where(p => p.PriceExVat >= minPrice.Value);
        if (maxPrice.HasValue) q = q.Where(p => p.PriceExVat <= maxPrice.Value);

        if (inStock == true)
            q = q.Where(p => (p.OnHand - p.Reserved) > 0);

        q = s switch
        {
            "price_asc" => q.OrderBy(p => p.PriceExVat),
            "price_desc" => q.OrderByDescending(p => p.PriceExVat),
            "name_asc" => q.OrderBy(p => EF.Functions.Collate(p.Name, "Latin1_General_100_BIN2")),
            "name_desc" => q.OrderByDescending(p => EF.Functions.Collate(p.Name, "Latin1_General_100_BIN2")),
            _ => q.OrderBy(p => p.Id),
        };

        var total = await q.CountAsync(ct);

        var items = await _assembler
            .ProjectToDto(q)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var result = new PagedResult<ProductDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };

        if (_cacheEnabled)
        {
            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result, CacheJson),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                },
                ct);
        }

        return result;
    }
    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"products:byid:{id}";


        if (_cacheEnabled)
        {

            var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
            if(cached is not null)
            {
                logger.LogInformation("Redis HIT {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<ProductDto>(cached, CacheJson);
            }
        }

        logger.LogInformation("Redis MISS {CacheKey}", cacheKey);

        var p = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.IsActive && p.Id == id, cancellationToken);

        if (p is null) return null;

        var dto = _assembler.ToDto(p);

        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(dto, CacheJson),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(120)
            },
            cancellationToken);

        return dto;
    }
    public async Task<IEnumerable<ProductSuggestionDto>> SuggestionAsync(string q, int take, CancellationToken ct)
    {
        var term = q.Trim();
        var size = Math.Clamp(take, 1, 20);

        var verSuggest = _cacheEnabled
            ? (await cache.GetStringAsync("products:ver:suggest", ct) ?? "0")
            : "0";

        var rawKey = $"ver={verSuggest}&q={term}&take={size}";
        var cacheKey = MakeKey("products:suggest", rawKey);

        if (_cacheEnabled)
        {
            var cached = await cache.GetStringAsync(cacheKey, ct);
            if (cached is not null)
            {
                logger.LogInformation("Redis HIT {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<List<ProductSuggestionDto>>(cached, CacheJson)!;
            }
        }


        logger.LogInformation("Redis MISS {CacheKey}", cacheKey);

        var result = await dbContext.Products
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
                p.Images
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.SortOrder)
                .Select(i => i.Url)
                .FirstOrDefault() ?? "",
                p.Slug ?? "",
                p.Sku ?? ""))
            .ToListAsync(ct);

        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result, CacheJson),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            },
            ct);

        return result;
    }

}

