namespace Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdmindashboardDto> GetAsync(AdminDashboardQueryDto query, CancellationToken ct);
}
