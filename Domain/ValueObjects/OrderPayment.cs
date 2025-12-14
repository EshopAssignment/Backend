
using Domain.Enums;

namespace Domain.ValueObjects;

public sealed record OrderPayment
{
    public PaymentStatus Status { get; set; } = PaymentStatus.RequiresPayment;

    public string? PaymentIntentId { get; private set; }
    public string? LatestChargeId { get; private set; }
    public string? PaymentMethodType { get;  private set; }
    
    
    public string Currency { get; private set; } = "SEK";
    public string? AmountAuthorized { get; private set; } = null!;
    public string? AmountCaptured { get; private set; } = null!;
    public string? AmountRefunded { get; private set; } = null!;

    public DateTime? AuthorizedAt { get; private set; }
    public DateTime? CapturedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    private OrderPayment() { } 

    public static OrderPayment Init(string currency) => new()
    {
        Currency = currency
    };

    public void OnIntentCreated(string paymentIntentId, string? methodeType)
    {
        PaymentIntentId = paymentIntentId;
        PaymentMethodType = methodeType ?? PaymentMethodType;
    }
    
    public void MarkAuthorized(string paymentIntentId, string? chargeId, decimal amount, string? methodType, DateTime nowUtc)
    {
        Status = PaymentStatus.Authorized;
        PaymentIntentId = paymentIntentId;
        LatestChargeId = chargeId;
        AmountAuthorized = amount.ToString("F2");
        PaymentMethodType = methodType ?? PaymentMethodType;
        AuthorizedAt = nowUtc;
    }

    public void MarkCaptured(decimal amount, DateTime nowUtc)
    {
        Status = PaymentStatus.Captured;
        AmountCaptured = amount.ToString("F2");
        CapturedAt = nowUtc;
    }

    public void MarkRefunded(decimal amount, DateTime nowUtc)
    {
        Status = PaymentStatus.Refunded;
        AmountRefunded = amount.ToString("F2");
        RefundedAt = nowUtc;
    }

    public void MarkFailed() => Status = PaymentStatus.Failed;
    public void MarkCancelled() => Status = PaymentStatus.Cancelled;
}
