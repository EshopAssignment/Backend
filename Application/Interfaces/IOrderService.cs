
using Application.DTOs.Order;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<OrderCreatedDto> CreateAsync(CreateOrderRequestDto dto, CancellationToken ct);
    Task<OrderCreatedDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<OrderCreatedDto?> GetByNumberAsync(string orderNumber, CancellationToken ct);


    Task<bool> MarkPaymentAuthorizedAsync(string orderNumber, string paymentIntentId, string? latestChargeId, string? methodType, decimal amount, string cartId, CancellationToken ct);
    Task<bool> MarkPaymentFailedAsync(string orderNumber, CancellationToken ct);
    Task<bool> MarkRefundedAsync(string orderNumber, decimal amount, CancellationToken ct);
}
