using Application.DTOs.Admin;
using Application.DTOs.Product;

namespace Application.Interfaces;

public interface IAdminProductService
{
    Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize, string? query, string? sort, List<string>? type, List<string>? condition, decimal? minPrice, decimal? maxPrice, bool? isActive, CancellationToken ct);
    Task<ProductDto> CreateAsync(AdminCreateProductRequestDto req, CancellationToken ct);
    Task<ProductDto> UpdateAsync(int id, AdminUpdateProductRequestDto req, CancellationToken ct);
    Task<bool> SetActiveAsync(int id, bool IsActive, CancellationToken ct);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

}