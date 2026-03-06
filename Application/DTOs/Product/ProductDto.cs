namespace Application.DTOs.Product;

public sealed record ProductDto
(
    int Id,
    string Name,
    string Description,
    string? PrimaryImgUrl,
    IReadOnlyList<ProductImageDto> Images,

    decimal PriceExVat,
    int VatRatePercent,

    string PalletType,     
    string Condition,      
    string StockStatus,    

    int OnHand,
    int Reserved,
    int Available,

    bool IsActive,
    string? Sku,
    string? Slug
);

