

namespace Application.Interfaces;
public interface IInventoryService
{
    Task<(bool ok, string? error)> ReserveAsync(int productId, int qty, string cartId, string? idempotencyKey, TimeSpan ttl, CancellationToken ct);
    Task ReleaseAsync(long reservationId, CancellationToken ct);
    Task<int> ReleaseExpiredAsync(CancellationToken ct);
    Task<(bool ok, string? error)> ConfirmOrderFromCartAsync(string cartId, string paymentKey, CancellationToken ct);
}
