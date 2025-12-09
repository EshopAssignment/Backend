using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<User, AppRole, int>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>().ToTable("Users", "auth");
        builder.Entity<AppRole>().ToTable("Roles", "auth");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles", "auth");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims", "auth");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims", "auth");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins", "auth");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens", "auth");

    }
}
