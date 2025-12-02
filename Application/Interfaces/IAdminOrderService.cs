

using Application.DTOs.Admin;
using Application.DTOs.Product;

namespace Application.Interfaces;

public interface IAdminOrderService
{
    Task<PagedResult<AdminOrderListItemDto>> GetAllAsync(int page, int pageSize, string? query, string? status, DateTime? from, DateTime? to, CancellationToken ct);
    Task<AdminOrderDetailsDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> UpdateStatusAsync(int id, string newStatus, CancellationToken ct);
}
