using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Entities.Users;
using ContentHub.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContentHub.IntegrationTests.Infrastructure;

public sealed class TestDataSeeder
{
    private readonly ContentHubApiFactory _factory;

    public TestDataSeeder(ContentHubApiFactory factory)
    {
        _factory = factory;
    }

    public async Task SeedRolesAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

        await EnsureRoleAsync(db, Roles.Admin, "Full access.");
        await EnsureRoleAsync(db, Roles.Editor, "Editor access.");
        await EnsureRoleAsync(db, Roles.Author, "Author access.");
        await EnsureRoleAsync(db, Roles.Viewer, "Viewer access.");

        await db.SaveChangesAsync();
    }

    public async Task<Guid> CreateUserAsync(
        string email,
        string username,
        string role)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var normalizedEmail = email.ToUpperInvariant();

        var existingUser = await db.Users
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail);

        if (existingUser is not null)
        {
            return existingUser.Id;
        }

        var roleEntity = await db.Roles
            .FirstOrDefaultAsync(x => x.NormalizedName == role.ToUpperInvariant());

        if (roleEntity is null)
        {
            roleEntity = new Role(role, $"{role} role.");
            db.Roles.Add(roleEntity);
            await db.SaveChangesAsync();
        }

        var user = new User(
            email: email,
            username: username,
            displayName: username,
            passwordHash: passwordHasher.Hash(TestConstants.DefaultPassword));

        user.MarkEmailAsVerified();
        user.AddRole(roleEntity.Id);

        db.Users.Add(user);

        await db.SaveChangesAsync();

        return user.Id;
    }

    public async Task SeedDefaultUsersAsync()
    {
        await CreateUserAsync(
            TestConstants.AdminEmail,
            TestConstants.AdminUsername,
            Roles.Admin);

        await CreateUserAsync(
            TestConstants.EditorEmail,
            TestConstants.EditorUsername,
            Roles.Editor);

        await CreateUserAsync(
            TestConstants.AuthorEmail,
            TestConstants.AuthorUsername,
            Roles.Author);
    }

    private static async Task EnsureRoleAsync(
        ContentHubDbContext db,
        string name,
        string description)
    {
        var normalizedName = name.ToUpperInvariant();

        var exists = await db.Roles
            .AnyAsync(role => role.NormalizedName == normalizedName);

        if (exists)
        {
            return;
        }

        db.Roles.Add(new Role(name, description));
    }
}