
namespace Application.DTOs.Admin;

public record AdminCreateProductRequestDto(

    int Id,
    string Name,
    string Description,
    string PalletType,
    string Condition,
    decimal Price,
    int StockQuantity,
    string ImgUrl,
    bool IsActive
);

public record AdminUpdateProductRequestDto(
    int Id,
    string Name,
    string Description,
    string PalletType,
    string Condition,
    decimal Price,
    int StockQuantity,
    string ImgUrl,
    bool IsActive
);

public record ToggleActiveRequest(bool IsActive);