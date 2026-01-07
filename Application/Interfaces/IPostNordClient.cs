using Application.DTOs.Shipping;

namespace Application.Interfaces
{
    public interface IPostNordClient
    {
        Task<IReadOnlyList<ServicePointDto>> FindServicePointsAsync(string postalCode, string? city, CancellationToken ct);
    }
}