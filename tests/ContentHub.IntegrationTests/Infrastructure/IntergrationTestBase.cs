using ContentHub.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContentHub.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected IntegrationTestBase(DatabaseFixture databaseFixture)
    {
        Factory = new ContentHubApiFactory(databaseFixture);
        Client = Factory.CreateClient();
        Auth = new AuthTestHelper(Client);
        Seeder = new TestDataSeeder(Factory);
        Cms = new CmsTestHelper(Client);
    }

    protected ContentHubApiFactory Factory { get; }

    protected HttpClient Client { get; }

    protected AuthTestHelper Auth { get; }

    protected TestDataSeeder Seeder { get; }
    
    protected CmsTestHelper Cms { get; }

    public virtual async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
        await Seeder.SeedDefaultUsersAsync();
        await ResetDatabaseAsync();
        await Factory.InitializeAsync();
        await Seeder.SeedDefaultUsersAsync();
    }
    
    public virtual async Task DisposeAsync()
    {
        Auth.ClearBearerToken();
        await Factory.DisposeAsync();
    }
    
    private async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

        await db.Database.ExecuteSqlRawAsync("""
                                                 TRUNCATE TABLE
                                                     audit_entity_changes,
                                                     audit_logs,
                                                     notification_deliveries,
                                                     notifications,
                                                     notification_preferences,
                                                     post_tags,
                                                     post_assets,
                                                     post_authors,
                                                     post_categories,
                                                     posts,
                                                     assets,
                                                     asset_versions,
                                                     authors,
                                                     categories,
                                                     refresh_tokens,
                                                     user_sessions,
                                                     user_roles,
                                                     users
                                                 RESTART IDENTITY CASCADE;
                                             """);
    }
    
}

