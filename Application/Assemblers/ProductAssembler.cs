using Application.DTOs.Product;
using Domain.Entities;

namespace Application.Assemblers;

public class ProductAssembler
{
    public ProductDto ToDto(Product p) =>
    new(
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
        p.Slug
    );

    public IQueryable<ProductDto> ProjectToDto(IQueryable<Product> query) =>
        query.Select(p => new ProductDto(
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
            p.Slug
            ));

}
