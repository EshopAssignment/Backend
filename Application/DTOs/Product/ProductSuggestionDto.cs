namespace Application.DTOs.Product;

public sealed record ProductSuggestionDto(
    int Id,
    string Name,
    decimal PriceExVat,
    string ImgUrl,
    string? Slug,
    string? Sku
    );
