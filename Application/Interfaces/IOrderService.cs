
using Application.DTOs.Order;
using Application.DTOs.Shipping;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<OrderCreatedDto> CreateAsync(CreateOrderRequestDto dto, int? userId, CancellationToken ct);
    Task<OrderCreatedDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<OrderCreatedDto?> GetByNumberAsync(string orderNumber, CancellationToken ct);


    Task<bool> MarkPaymentAuthorizedAsync(string orderNumber, string paymentIntentId, string? latestChargeId, string? methodType, decimal amount, string cartId, CancellationToken ct);
    Task<bool> MarkPaymentFailedAsync(string orderNumber, CancellationToken ct);
    Task<bool> MarkRefundedAsync(string orderNumber, decimal amount, CancellationToken ct);

    Task<bool> SetShippingSelectionAsync(string orderNumber, SetShippingSelectionDto dto, CancellationToken ct);

    Task<bool> UpdateCustomerAsync(string orderNumber, UpdateOrderCustomerDto dto, int? userId, CancellationToken ct);
    Task<bool> UpdateShippingAddressAsync(string orderNumber, UpdateOrderShippingAddressDto dto, int? userId, CancellationToken ct);

    Task<IReadOnlyList<MyOrderListItemDto>> GetMyOrdersAsync(int userId, int skip, int take, CancellationToken ct);
}
