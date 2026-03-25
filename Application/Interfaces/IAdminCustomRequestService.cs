
using Application.DTOs.Admin;
using Application.DTOs.Product;

namespace Application.Interfaces;

public interface IAdminCustomRequestService
{
    Task<PagedResult<AdminCustomRequestListItemDto>> GetAllAsync(
        int page,
        int pageSize,
        string? query,
        string? status,
        CancellationToken ct);
    Task<AdminCustomRequestDetailsDto> GetByIdAsync(int id, CancellationToken ct);
    Task<AdminCustomQuoteDetailsDto> CreateQuoteAsync(int CustomRequestId, AdminCreateCustomQuoteDto dto, CancellationToken ct);
    Task<bool> SendQuoteAsync(int quoteId, CancellationToken ct);
}
