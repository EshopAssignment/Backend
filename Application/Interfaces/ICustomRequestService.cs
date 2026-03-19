using Application.DTOs.Order;

namespace Application.Interfaces;

public interface ICustomRequestService
{
    Task<(bool Ok, string? Error)> CreateAsync(CreateCustomRequestForm form, CancellationToken ct);
}
