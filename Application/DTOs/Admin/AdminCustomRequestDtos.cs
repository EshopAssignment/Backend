using Domain.Enums;

namespace Application.DTOs.Admin;
//List

public sealed record AdminCustomRequestListItemDto(
    int Id,
    DateTime CreatedAtUtc,
    string Name,
    string Email,
    string? Phone,
    CustomRequestStatus Status,
    bool HasAttachment
);
public sealed record AdminCustomQuoteListItemDto(
    int Id,
    DateTime CreatedAtUtc,
    string Title,
    CustomQuoteStatus Status,
    decimal TotalIncVat,
    DateTime? SentAtUtc,
    DateTime? ExpiresAtUtc
);
public sealed record AdminCustomRequestDetailsDto(
    int Id,
    DateTime CreatedAtUtc,
    string Name,
    string Email,
    string? Phone,
    string Message,
    CustomRequestStatus Status,
    string? AttachmentFileName,
    string? AttachmentBlobPath,
    string? InternalNote,
    IReadOnlyList<AdminCustomQuoteListItemDto> Quotes
);

//Send

public sealed record AdminCreateCustomQuoteItemDto(
    string Description,
    int Quantity,
    decimal UnitPriceExVat,
    int VatRatePercent
);
public sealed record AdminCreateCustomQuoteDto(
    string Title,
    string? CustomerMessage,
    string? InternalNote,
    DateTime? ExpiresAtUtc,
    List<AdminCreateCustomQuoteItemDto> Items
);
public sealed record AdminCustomQuoteItemDto(
    string Description,
    int Quantity,
    decimal UnitPriceExVat,
    int VatRatePercent,
    decimal UnitVatAmount,
    decimal UnitPriceIncVat,
    decimal LineTotalExVat,
    decimal LineTotalVat,
    decimal LineTotalIncVat
);
public sealed record AdminCustomQuoteDetailsDto(
    int Id,
    int CustomRequestId,
    string Title,
    string Currency,
    string? CustomerMessage,
    string? InternalNote,
    DateTime CreatedAtUtc,
    DateTime? SentAtUtc,
    DateTime? ExpiresAtUtc,
    string Status,
    decimal SubtotalExVat,
    decimal VatTotal,
    decimal TotalIncVat,
    IReadOnlyList<AdminCustomQuoteItemDto> Items
);