using ContentHub.Data.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Data.Persistence.Seed;

public sealed class RoleSeeder
{
    public async Task SeedAsync(
        ContentHubDbContext db,
        CancellationToken cancellationToken = default)
    {
        await SeedRoleAsync(db, "Admin", "Full system access.", cancellationToken);
        await SeedRoleAsync(db, "Editor", "Can manage and publish content.", cancellationToken);
        await SeedRoleAsync(db, "Author", "Can create and manage own content.", cancellationToken);
        await SeedRoleAsync(db, "Viewer", "Can view content.", cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRoleAsync(
        ContentHubDbContext db,
        string name,
        string description,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.ToUpperInvariant();

        var exists = await db.Roles
            .AnyAsync(role => role.NormalizedName == normalizedName, cancellationToken);

        if (exists)
        {
            return;
        }

        db.Roles.Add(new Role(name, description));
    }
}