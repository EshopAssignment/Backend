namespace Application.DTOs.Product;

public sealed record ProductSuggestionDto(
    int Id,
    string Name,
    decimal PriceExVat,
    string? PrimaryImgUrl,
    IReadOnlyList<ProductImageDto> Images
    string? Slug,
    string? Sku
    );
