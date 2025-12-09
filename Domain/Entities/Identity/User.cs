using Microsoft.AspNetCore.Identity;
namespace Domain.Entities.Identity;

public class User : IdentityUser<int>
{
    public string? DisplayName { get; set; }
}

