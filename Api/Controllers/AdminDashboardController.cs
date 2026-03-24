using Application.DTOs.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController(IAdminDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminDashboardDto))]
    public async Task<IActionResult> Get(
        [FromQuery] string range = "30d",
        [FromQuery] DateTime? fromUtc  = null,
        [FromQuery] DateTime? toUtc = null,
        CancellationToken ct = default)
    {
        var dto = await dashboardService.GetAsync(
            new AdminDashboardQueryDto(range, fromUtc, toUtc), ct);

        return Ok(dto);
    }
}
