using Application.DTOs.Product;
using Domain.Entities;
using Domain.Enums;

namespace Application.Assemblers;

public class ProductAssembler
{
    public ProductDto ToDto(Product p)
    {
        var images = p.Images
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.SortOrder)
            .Select(x => new ProductImageDto(
                x.Id,
                x.Url,
                x.SortOrder,
                x.IsPrimary,
                x.AltText))
            .ToList();

        var primary = images.FirstOrDefault()?.Url;
        var available = Math.Max(0, p.OnHand - p.Reserved);
        var stockStatus =
            available <= 0 ? StockStatus.OutOfStock :
            available <= p.LowStockThreshold ? StockStatus.LowStock :
            StockStatus.InStock;

        return new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            primary,
            images,
            p.PriceExVat,
            (int)p.VatRate,
            p.PalletType.ToString(),
            p.StockStatus.ToString(),
            p.Condition.ToString(),
            p.OnHand,
            p.Reserved,
            available,
            p.IsActive,
            p.Sku,
            p.Slug);
    }

    public IQueryable<ProductDto> ProjectToDto(IQueryable<Product> query) => 
        query.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Images
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.SortOrder)
            .Select(i => i.Url)
            .FirstOrDefault(),
            
            p.Images
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.SortOrder)
            .Select(i => new ProductImageDto(
                i.Id,
                i.Url,
                i.SortOrder,
                i.IsPrimary,
                i.AltText))
            .ToList(),

            p.PriceExVat,
            (int)p.VatRate,
            p.PalletType.ToString(),
            p.Condition.ToString(),
            
            (Math.Max(0, p.OnHand - p.Reserved) <= 0
                ? StockStatus.OutOfStock
                : (Math.Max(0, p.OnHand - p.Reserved) <= p.LowStockThreshold
                    ? StockStatus.LowStock
                    : StockStatus.InStock
                )
            ).ToString(),

            p.OnHand,
            p.Reserved,
            Math.Max(0, p.OnHand - p.Reserved),
            
            p.IsActive,
            p.Sku,
            p.Slug));
}
