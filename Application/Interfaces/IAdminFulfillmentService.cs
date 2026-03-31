using Application.DTOs;
using Application.DTOs.Admin;

namespace Application.Interfaces;

public interface IAdminFulfillmentService
{
    Task<PagedResultDto<AdminFulfillmentOrderDto>> GetQueueAsync(
          FulfillmentQueueFilterDto filter,
          CancellationToken ct = default);

    Task<AdminFulfillmentDashboardDto> GetDashboardAsync(
        CancellationToken ct = default);

    Task<AdminFulfillmentOrderDto?> GetByIdAsync(
        int orderId,
        CancellationToken ct = default);

    Task MarkFulfilledAsync(
        int orderId,
        string? note,
        CancellationToken ct = default);

    Task ReopenAsync(
        int orderId,
        string? note,
        CancellationToken ct = default);

    Task SetFulfillmentNoteAsync(
        int orderId,
        string? note,
        CancellationToken ct = default);
}
