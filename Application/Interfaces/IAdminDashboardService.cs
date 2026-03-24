using Application.DTOs.Admin;

namespace Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetAsync(AdminDashboardQueryDto query, CancellationToken ct);
}
