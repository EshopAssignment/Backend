using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<User, AppRole, int>(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
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

        builder.Entity<UserProfile>(b =>
        {
            b.ToTable("UserProfiles", "auth");
            b.HasKey(x => x.UserId);

            b.Property(x => x.FirstName).HasMaxLength(100);
            b.Property(x => x.LastName).HasMaxLength(100);
            b.Property(x => x.Phone).HasMaxLength(30);

            b.HasOne(x => x.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.Addresses)
                .WithOne(a => a.Profile)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.DefaultShippingAddress)
                .WithMany()
                .HasForeignKey(x => x.DefaultShippingAddressId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<UserAddress>(b =>
        {
            b.ToTable("UserAddresses", "auth");
            b.HasKey(x => x.Id);

            b.Property(x => x.Label).HasMaxLength(50);
            b.Property(x => x.Street).HasMaxLength(200);
            b.Property(x => x.City).HasMaxLength(100);
            b.Property(x => x.PostalCode).HasMaxLength(20);
            b.Property(x => x.Country).HasMaxLength(2);
        });

    }
}
