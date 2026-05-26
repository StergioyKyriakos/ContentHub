namespace ContentHub.Data.Persistence.Seed;

public sealed class DatabaseSeeder
{
    private readonly RoleSeeder _roleSeeder;
    private readonly AdminUserSeeder _adminUserSeeder;

    public DatabaseSeeder(
        RoleSeeder roleSeeder,
        AdminUserSeeder adminUserSeeder)
    {
        _roleSeeder = roleSeeder;
        _adminUserSeeder = adminUserSeeder;
    }

    public async Task SeedAsync(
        ContentHubDbContext db,
        CancellationToken cancellationToken = default)
    {
        await _roleSeeder.SeedAsync(db, cancellationToken);
        await _adminUserSeeder.SeedAsync(db, cancellationToken);
    }
}