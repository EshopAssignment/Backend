
using Application.Interfaces;

namespace IntegrationTests.Infrastructure.Fakes;

public sealed class FakeInventoryService : IInventoryService
{

    public bool VerifyShouldFail { get; set; } = false;
    public string VerifyFailMessage { get; set; } = "RESERVATION_MISMATCH";

    public bool ConfirmShouldFail { get; set; } = false;
    public string ConfirmFailMessage { get; set; } = "CONFIRM_FAILED";

    public Task<(bool ok, string? error)> ConfirmOrderFromCartAsync(string cartId, string paymentKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cartId))
            return Task.FromResult<(bool, string?)>((false, "CartId is required"));

        if (string.IsNullOrWhiteSpace(paymentKey))
            return Task.FromResult<(bool, string?)>((false, "paymentKey is required"));

        if (ConfirmShouldFail)
            return Task.FromResult<(bool, string?)>((false, ConfirmFailMessage));

        return Task.FromResult<(bool, string?)>((true, null));
    }

    public Task ReleaseAsync(long reservationId, CancellationToken ct)
    => Task.CompletedTask;

    public Task<int> ReleaseExpiredAsync(CancellationToken ct)
    => Task.FromResult(0);

    public Task<(bool ok, string? error)> ReserveAsync(int productId, int qty, string cartId, string? idempotencyKey, TimeSpan ttl, CancellationToken ct)
    => Task.FromResult<(bool, string?)>((true, null));

    public Task<(bool ok, string? error)> SetReservationQtyAsync(int productId, int desiredQty, string cartId, TimeSpan ttl, CancellationToken ct)
    => Task.FromResult<(bool, string?)>((true, null));

    public Task<(bool ok, string? error)> VerifyCartReservationAsync(string cartId, IReadOnlyList<(int productId, int qty)> items, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cartId))
            return Task.FromResult<(bool, string?)>((false, "CartId is required"));

        if (items is null || items.Count == 0)
            return Task.FromResult<(bool, string?)>((false, "Must at least have one item"));

        if (items.Any(i => i.qty <= 0))
            return Task.FromResult<(bool, string?)>((false, "item quantity must be >= 1"));

        if (VerifyShouldFail)
            return Task.FromResult<(bool, string?)>((false, VerifyFailMessage));

        return Task.FromResult<(bool, string?)>((true, null));
    }
}
