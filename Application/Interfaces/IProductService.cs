using Application.DTOs;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResult<ProductDto>> GetAllPagedAsync(int page, int pageSize, string? query, string? sort, List<string>? type, List<string>? condition, decimal? minPrice, decimal? maxPrice, CancellationToken ct);
    }
}