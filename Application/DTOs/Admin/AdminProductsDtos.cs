
namespace Application.DTOs.Admin;

public record AdminCreateProductRequestDto(
    string Name,
    string Description,
    string PalletType,
    string Condition,
    decimal PriceExVat,
    int OnHand,
    string ImgUrl,
    bool IsActive
);

public record AdminUpdateProductRequestDto(
    int Id,
    string Name,
    string Description,
    string PalletType,
    string Condition,
    decimal PriceExVat,
    int OnHand,
    string ImgUrl,
    bool IsActive
);

public record ToggleActiveRequest(bool IsActive);