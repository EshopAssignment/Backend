namespace Domain.Enums;

public enum PaymentStatus
{
    None = 0,
    RequiresPayment = 1,
    Authorized = 2,
    Captured = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6
}

