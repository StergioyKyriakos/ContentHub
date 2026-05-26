using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Data.Persistence.Seed;

public sealed class AdminUserSeeder
{
    private const string AdminEmail = "admin@contenthub.local";
    private const string AdminUsername = "admin";
    private const string AdminDisplayName = "Admin";
    private const string AdminPassword = "Admin123!";

    private readonly IPasswordHasher _passwordHasher;

    public AdminUserSeeder(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(
        ContentHubDbContext db,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = AdminEmail.ToUpperInvariant();

        var exists = await db.Users
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (exists)
        {
            return;
        }

        var adminRole = await db.Roles
            .FirstOrDefaultAsync(role => role.NormalizedName == "ADMIN", cancellationToken);

        if (adminRole is null)
        {
            throw new InvalidOperationException("Admin role was not seeded.");
        }

        var passwordHash = _passwordHasher.Hash(AdminPassword);

        var admin = new User(
            email: AdminEmail,
            username: AdminUsername,
            displayName:AdminDisplayName ,
            passwordHash: passwordHash);

        admin.MarkEmailAsVerified();
        admin.AddRole(adminRole.Id);

        db.Users.Add(admin);

        await db.SaveChangesAsync(cancellationToken);
    }
}