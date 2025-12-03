using Application.DTOs.Product;

namespace Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductSuggestionDto>> SuggestionAsync(string q, int take, CancellationToken ct);
    Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize, string? query, string? sort, List<string>? type, List<string>? condition, decimal? minPrice, decimal? maxPrice, CancellationToken ct);
   
}
