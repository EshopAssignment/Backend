
namespace Application.DTOs.Product;

public sealed record ProductImageDto(
    int Id,
    string OriginalUrl,
    string LargeUrl,
    string CardUrl,
    string StackUrl,
    string ThumbUrl,
    int SortOrder,
    bool IsPrimary,
    string? AltText
    );
