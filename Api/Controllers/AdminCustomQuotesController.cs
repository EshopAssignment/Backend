using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/admin/custom-quotes")]
public class AdminCustomQuotesController(IAdminCustomRequestService service) : ControllerBase
{
    [HttpPost("{id:int}/send")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Send(int id, CancellationToken ct = default)
    {
        var ok = await service.SendQuoteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
