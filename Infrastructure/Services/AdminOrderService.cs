
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.DTOs.Admin;
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AdminOrderService(
    PallshoppenDbContext dbContext,
    IDistributedCache cache,
    IConfiguration config,
    ILogger<AdminOrderService> logger) : IAdminOrderService
{
    private readonly bool _cacheEnabled = config?.GetValue("Cache:Enabled", true)
          ?? throw new ArgumentNullException(nameof(config), "IConfiguration is null. AdminOrderService is likely being constructed manually instead of via DI.");
    private static readonly JsonSerializerOptions CacheJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<PagedResult<AdminOrderListItemDto>> GetAllAsync(
      int page, int pageSize, string? query, string? status, DateTime? from, DateTime? to, CancellationToken ct)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 200 ? 200 : pageSize;

        var qTerm = query?.Trim();
        var sNorm = status?.Trim();

        var ver = _cacheEnabled
            ? (await cache.GetStringAsync("orders:ver:adminlist", ct) ?? "0")
            : "0";

        var fromKey = from.HasValue ? from.Value.ToUniversalTime().ToString("O") : "";
        var toKey = to.HasValue ? to.Value.ToUniversalTime().ToString("O") : "";

        var rawKey =
            $"ver={ver}&page={page}&pageSize={pageSize}&q={qTerm}&status={sNorm}&from={fromKey}&to={toKey}";

        var cacheKey = MakeKey("orders:adminlist", rawKey);

        if (_cacheEnabled)
        {
            var cached = await cache.GetStringAsync(cacheKey, ct);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                try
                {
                    logger.LogInformation("Redis HIT {CacheKey}", cacheKey);
                    return JsonSerializer.Deserialize<PagedResult<AdminOrderListItemDto>>(cached, CacheJson)
                           ?? throw new JsonException("Deserialized null");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Redis DESERIALIZE FAILED {CacheKey}", cacheKey);
                }
            }

            logger.LogInformation("Redis MISS {CacheKey}", cacheKey);
        }

        var q = dbContext.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(qTerm))
        {
            q = q.Where(o =>
                o.OrderNumber.Contains(qTerm) ||
                (o.CustomerFirstName != null && o.CustomerFirstName.Contains(qTerm)) ||
                (o.CustomerLastName != null && o.CustomerLastName.Contains(qTerm)) ||
                (o.CustomerEmail != null && o.CustomerEmail.Contains(qTerm)) ||
                (o.CustomerPhoneNumber != null && o.CustomerPhoneNumber.Contains(qTerm)));
        }

        if (!string.IsNullOrWhiteSpace(sNorm) &&
            Enum.TryParse<OrderStatus>(sNorm, ignoreCase: true, out var parsed))
        {
            q = q.Where(o => o.OrderStatus == parsed);
        }

        if (from.HasValue) q = q.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(o => o.CreatedAt <= to.Value);

        q = q.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id);

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new AdminOrderListItemDto(
                Id: o.Id,
                OrderNumber: o.OrderNumber,
                CreatedAtUtc: o.CreatedAt,
                CustomerName: ((o.CustomerFirstName ?? "") + " " + (o.CustomerLastName ?? "")).Trim(),
                CustomerEmail: o.CustomerEmail ?? string.Empty,
                OrderStatus: o.OrderStatus,
                PaymentStatus: o.Payment.Status,
                GrandTotal: o.GrandTotal,
                PaymentMethod: o.Payment.PaymentMethodType ?? string.Empty
            ))
            .ToListAsync(ct);

        var result = new PagedResult<AdminOrderListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize),
            Items = items
        };

        if (_cacheEnabled)
        {
            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result, CacheJson),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                },
                ct);
        }

        return result;
    }
    public async Task<AdminOrderDetailsDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var cacheKey = $"orders:admin:byid:{id}";

        if (_cacheEnabled)
        {
            var cached = await cache.GetStringAsync(cacheKey, ct);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                try
                {
                    logger.LogInformation("Redis HIT {CacheKey}", cacheKey);
                    return JsonSerializer.Deserialize<AdminOrderDetailsDto>(cached, CacheJson)
                           ?? throw new JsonException("Deserialized null");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Redis DESERIALIZE FAILED {CacheKey}", cacheKey);
                }
            }

            logger.LogInformation("Redis MISS {CacheKey}", cacheKey);
        }

        var o = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (o is null) return null;

        var items = o.OrderItems
            .OrderBy(i => i.Id)
            .Select(i => new AdminOrderItemDto(
                ProductId: i.ProductId,
                Sku: i.Sku,
                ProductName: i.ProductName,

                UnitPriceExVat: i.UnitPriceExVat,
                VatRatePercent: i.VatRatePercent,
                UnitVatAmount: i.UnitVatAmount,
                UnitPriceIncVat: i.UnitPriceIncVat,

                Quantity: i.Quantity,

                LineTotalExVat: i.LineTotalExVat,
                LineTotalVat: i.LineTotalVat,
                LineTotalIncVat: i.LineTotalIncVat
            ))
            .ToList();

        var street = o.ShippingAddress?.Street ?? string.Empty;
        var postal = o.ShippingAddress?.PostalCode ?? string.Empty;
        var city = o.ShippingAddress?.City ?? string.Empty;
        var country = o.ShippingAddress?.Country ?? string.Empty;

        var trackingNumber = o.TrackingNumber;
        var trackingUrl = string.IsNullOrWhiteSpace(trackingNumber)
            ? null
            : $"https://tracking.postnord.com/?id={Uri.EscapeDataString(trackingNumber)}";

        var dto = new AdminOrderDetailsDto(
            Id: o.Id,
            OrderNumber: o.OrderNumber,
            CreatedAtUtc: o.CreatedAt,

            CustomerFirstName: o.CustomerFirstName ?? string.Empty,
            CustomerLastName: o.CustomerLastName ?? string.Empty,
            CustomerEmail: o.CustomerEmail ?? string.Empty,
            CustomerPhoneNumber: o.CustomerPhoneNumber ?? string.Empty,

            ShippingStreet: street,
            ShippingPostalCode: postal,
            ShippingCity: city,
            ShippingCountry: country,

            OrderStatus: o.OrderStatus,
            PaymentStatus: o.Payment.Status,
            PaymentMethod: o.Payment.PaymentMethodType ?? string.Empty,
            PaymentIntentId: o.Payment.PaymentIntentId,

            Currency: o.Currency,

            ProductsSubtotal: o.ProductsSubtotal,
            ShippingCost: o.ShippingCost,
            VatTotal: o.VatTotal,
            GrandTotal: o.GrandTotal,

            TrackingNumber: trackingNumber,
            TrackingUrl: trackingUrl,

            Items: items
        );

        if (_cacheEnabled)
        {
            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(dto, CacheJson),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20)
                },
                ct);
        }

        return dto;
    }
    public async Task<bool> UpdateStatusAsync(int id, string newStatus, CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(newStatus, ignoreCase: true, out var next))
            return false;

        var o = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return false;

        switch (next)
        {
            case OrderStatus.Processing: o.MarkProcessing(); break;
            case OrderStatus.Shipped: o.MarkShipped(); break;
            case OrderStatus.Completed: o.MarkCompleted(); break;
            case OrderStatus.Cancelled: o.MarkCancelled(); break;
            case OrderStatus.Confirmed: o.MarkConfirmed(); break;
            case OrderStatus.Failed: o.MarkFailed(); break;
            case OrderStatus.Refunded: o.MarkRefunded(); break;

            case OrderStatus.Pending:
                return false;

            default: return false;
        }

        await dbContext.SaveChangesAsync(ct);

        if (_cacheEnabled)
        {
            await cache.RemoveAsync($"orders:admin:byid:{id}", ct);
            await InvalidateAdminListAsync(cache, ct);
        }


        return true;
    }
    public async Task<bool> SetTrackingAsync(int id, string trackingNumber, bool markAsShipped, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            return false;

        var o = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return false;

        o.SetTracking(trackingNumber);

        if (markAsShipped)
            o.MarkShipped();

        await dbContext.SaveChangesAsync(ct);

        if (_cacheEnabled)
        {
            await cache.RemoveAsync($"orders:admin:byid:{id}", ct);
            await InvalidateAdminListAsync(cache, ct);
        }


        return true;
    }
    //caching Task
    private static string MakeKey(string prefix, string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return $"{prefix}:{Convert.ToHexString(bytes)}";
    }
    private static Task BumpAsync(IDistributedCache cache, string key, CancellationToken ct)
        => cache.SetStringAsync(key, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(), ct);

    private static Task InvalidateAdminListAsync(IDistributedCache cache, CancellationToken ct)
        => BumpAsync(cache, "orders:ver:adminlist", ct);

}
