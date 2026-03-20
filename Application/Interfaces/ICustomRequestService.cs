using Application.DTOs.Order;

namespace Application.Interfaces;

public interface ICustomRequestService
{
    Task<(bool Ok, string? Error)> CreateAsync(CreateCustomRequestFormDto form, CancellationToken ct);
}
