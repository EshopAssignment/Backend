
namespace Application.DTOs.Product;

public sealed record ProductImageDto(
    int Id,
    string Url,
    int SortOrder,
    bool IsPrimary,
    string? AltText
    );
