using System.Net.Http.Json;
using ContentHub.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected IntegrationTestBase(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : this(
            databaseFixture,
            output,
            null)
    {
    }

    protected IntegrationTestBase(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output,
        ContentHubApiFactoryOptions? factoryOptions)
    {
        Output = output;
        Factory = new ContentHubApiFactory(databaseFixture, factoryOptions);
        Client = Factory.CreateClient();
        Auth = new AuthTestHelper(Client, output);
        Seeder = new TestDataSeeder(Factory);
        Cms = new CmsTestHelper(Client, output);
    }

    protected ContentHubApiFactory Factory { get; }

    protected ITestOutputHelper Output { get; }

    protected HttpClient Client { get; }

    protected AuthTestHelper Auth { get; }

    protected TestDataSeeder Seeder { get; }
    
    protected CmsTestHelper Cms { get; }

    protected async Task<string> LogResponseAsync(
        HttpResponseMessage response,
        string label)
    {
        var body = await response.Content.ReadAsStringAsync();

        Output.WriteLine(label);
        Output.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
        Output.WriteLine(body);

        return body;
    }

    protected Task<HttpResponseMessage> GetAsJsonAsync(string url, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Content = JsonContent.Create(body)
        };

        return Client.SendAsync(request);
    }

    protected Task<HttpResponseMessage> DeleteAsJsonAsync(string url, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(body)
        };

        return Client.SendAsync(request);
    }

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
                                                     email_verification_tokens,
                                                     password_reset_tokens,
                                                     refresh_tokens,
                                                     user_sessions,
                                                     user_roles,
                                                     users
                                                 RESTART IDENTITY CASCADE;
                                             """);
    }
    
}

