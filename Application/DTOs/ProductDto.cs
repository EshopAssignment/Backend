namespace Application.DTOs;

public sealed record ProductDto
(
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

