
namespace Contracts.Events;

public sealed record OrderCreatedEvent(
    int OrderId,
    string OrderNUmber,
    int? UserId,
    string CartId,
    DateTime CreatedAtUtc);

public sealed record OrderConfirmedEvent(
    int OrderId,
    string OrderNumber,
    DateTime ConfirmedUtc);

public sealed record OrderStatusChangedEvent(
    int OrderId,
    string OrderNumber,
    string FromStatus,
    string ToStatus,
    DateTime ChangedAtUtc);

public sealed record OrderTrackingSetEvent(
    int OrderId,
    string OrderNumber,
    string TrackingNumber,
    bool MarkedAsShipped,
    DateTime SetAtUtc);