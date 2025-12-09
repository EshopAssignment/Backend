using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Seed;

public class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        foreach (var r in new[] { "Admin", "User" })
            if (!await roles.RoleExistsAsync(r)) await roles.CreateAsync(new AppRole { Name = r });

        var adminEmail = "admin@admin.com";
        var admin = await users.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new User { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, DisplayName = "Admin" };
            await users.CreateAsync(admin, "Bytmig123!");
            await users.AddToRoleAsync(admin, "Admin");
        }
    }
}
