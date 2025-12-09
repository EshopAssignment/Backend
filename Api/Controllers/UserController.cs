using System.Security.Claims;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("users")]
public class UserController(UserManager<User> users) : ControllerBase
{
    private readonly UserManager<User> _users = users;

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id)) return Unauthorized();

        var u = await _users.FindByIdAsync(id);
        if (u is null) return Unauthorized(); 

        var roles = await _users.GetRolesAsync(u);
        return Ok(new { u.Email, u.DisplayName, roles });
    }
}
