using Application.DTOs.Admin;

namespace Application.Interfaces;

public interface IAdminFulfillmentService
{
    Task<IReadOnlyList<AdminFulfillmentOrderDto>> GetQueueAsync(bool overdueOnly, CancellationToken ct = default);
    Task<AdminFulfillmentDashboardDto> GetDashboardAsync(CancellationToken ct = default);
    Task MarkFulfilledAsync(int OrderId, string? note, CancellationToken ct = default);
    Task ReopenAsync(int orderId, string? note, CancellationToken ct = default);
}
