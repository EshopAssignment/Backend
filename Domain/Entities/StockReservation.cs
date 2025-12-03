
using Domain.Enums;

namespace Domain.Entities;

public class StockReservation
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public string CartId { get; set; } = "";
    public DateTimeOffset ExpiresAt { get; set; }
    public StockReservationStatus Status { get; set; } = StockReservationStatus.Active;
    public string? IdempotencyKey { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

}
