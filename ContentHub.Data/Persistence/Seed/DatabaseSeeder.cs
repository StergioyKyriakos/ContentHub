namespace ContentHub.Data.Persistence.Seed;

public sealed class DatabaseSeeder
{
    private readonly RoleSeeder _roleSeeder;
    private readonly AdminUserSeeder _adminUserSeeder;
    private readonly ContentSeeder _contentSeeder;

    public DatabaseSeeder(
        RoleSeeder roleSeeder,
        AdminUserSeeder adminUserSeeder,
        ContentSeeder contentSeeder)
    {
        _roleSeeder = roleSeeder;
        _adminUserSeeder = adminUserSeeder;
        _contentSeeder = contentSeeder;
    }

    public async Task SeedAsync(
        ContentHubDbContext db,
        CancellationToken cancellationToken = default)
    {
        await _roleSeeder.SeedAsync(db, cancellationToken);
        await _adminUserSeeder.SeedAsync(db, cancellationToken);
        await _contentSeeder.SeedAsync(db, cancellationToken);
    }
}
