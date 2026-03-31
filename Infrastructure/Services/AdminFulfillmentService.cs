using Application.DTOs;
using Application.DTOs.Admin;
using Application.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Services;

public class AdminFulfillmentService(PallshoppenDbContext dbContext) : IAdminFulfillmentService
{
    private const int OverDuaAfterDays = 7;
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public Task<PagedResultDto<AdminFulfillmentOrderDto>> GetQueueAsync(FulfillmentQueueFilterDto filter, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
    public Task<AdminFulfillmentOrderDto?> GetByIdAsync(int orderId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<AdminFulfillmentDashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }



    public Task MarkFulfilledAsync(int orderId, string? note, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task ReopenAsync(int orderId, string? note, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SetFulfillmentNoteAsync(int orderId, string? note, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
