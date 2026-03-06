using Application.DTOs.Options;

namespace Application.DTOs.Admin;

public record AdminCreateProductRequestDto(
    string Name,
    string Description,
    string PalletType,
    string Condition,
    decimal PriceExVat,
    int VatRatePercent,
    int OnHand,
    List<AdminProductImageRequestDto> Images,
    bool IsActive
);

public record AdminUpdateProductRequestDto(
    int Id,
    string Name,
    string Description,
    string PalletType,
    string Condition,
    decimal PriceExVat,
    int VatRatePercent,
    int OnHand,
    List<AdminProductImageRequestDto> Images,
    bool IsActive
);

public record AdminProductImageRequestDto(
    string Url,
    int SortOrder,
    bool IsPrimary,
    string? AltText);


public record AdminProductOptionsDto(
    IEnumerable<EnumOptionDto> ProductTypes, IEnumerable<EnumOptionDto> ProductConditions, IEnumerable<EnumOptionDto> VatRates);
public record ToggleActiveRequest(bool IsActive);