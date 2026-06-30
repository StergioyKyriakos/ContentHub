using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContentHub.IntegrationTests.Infrastructure;

public sealed class ContentHubApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly ContentHubApiFactoryOptions _options;

    public ContentHubApiFactory(
        DatabaseFixture databaseFixture,
        ContentHubApiFactoryOptions? options = null)
    {
        _databaseFixture = databaseFixture;
        _options = options ?? new ContentHubApiFactoryOptions();

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        Environment.SetEnvironmentVariable("ConnectionStrings__Database", _databaseFixture.ConnectionString);

        Environment.SetEnvironmentVariable("Jwt__Issuer", "ContentHub.Tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "ContentHub.Tests");
        Environment.SetEnvironmentVariable("Jwt__Secret", "THIS_IS_A_TEST_SECRET_KEY_FOR_INTEGRATION_TESTS_123456789");
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
        Environment.SetEnvironmentVariable("Jwt__RefreshTokenExpirationDays", "30");

        Environment.SetEnvironmentVariable("Storage__Provider", "Local");
        Environment.SetEnvironmentVariable("Storage__LocalRootPath", "storage/test-assets");
        Environment.SetEnvironmentVariable("Storage__PublicBaseUrl", "/assets");
        Environment.SetEnvironmentVariable("Storage__MaxFileSizeBytes", "10485760");
        Environment.SetEnvironmentVariable("Storage__AllowedContentTypes__0", "image/jpeg");
        Environment.SetEnvironmentVariable("Storage__AllowedContentTypes__1", "image/png");
        Environment.SetEnvironmentVariable("Storage__AllowedContentTypes__2", "image/webp");
        Environment.SetEnvironmentVariable("Storage__AllowedContentTypes__3", "image/gif");
        Environment.SetEnvironmentVariable("Storage__AllowedContentTypes__4", "application/pdf");

        Environment.SetEnvironmentVariable("Redis__Enabled", _options.RedisEnabled ? "true" : "false");
        Environment.SetEnvironmentVariable("BackgroundJobs__Enabled", _options.BackgroundJobsEnabled ? "true" : "false");
        Environment.SetEnvironmentVariable("Outbox__Enabled", _options.OutboxEnabled ? "true" : "false");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _databaseFixture.ConnectionString,

                ["Jwt:Issuer"] = "ContentHub.Tests",
                ["Jwt:Audience"] = "ContentHub.Tests",
                ["Jwt:Secret"] = "THIS_IS_A_TEST_SECRET_KEY_FOR_INTEGRATION_TESTS_123456789",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "30",

                ["Seed:Admin:Password"] = TestConstants.DefaultPassword,

                ["Storage:Provider"] = "Local",
                ["Storage:LocalRootPath"] = "storage/test-assets",
                ["Storage:PublicBaseUrl"] = "/assets",
                ["Storage:MaxFileSizeBytes"] = "10485760",
                ["Storage:AllowedContentTypes:0"] = "image/jpeg",
                ["Storage:AllowedContentTypes:1"] = "image/png",
                ["Storage:AllowedContentTypes:2"] = "image/webp",
                ["Storage:AllowedContentTypes:3"] = "image/gif",
                ["Storage:AllowedContentTypes:4"] = "application/pdf",

                ["Redis:Enabled"] = _options.RedisEnabled ? "true" : "false",
                ["BackgroundJobs:Enabled"] = _options.BackgroundJobsEnabled ? "true" : "false",
                ["BackgroundJobs:ScheduledPostPublisher:Enabled"] = _options.BackgroundJobsEnabled ? "true" : "false",
                ["BackgroundJobs:ScheduledPostPublisher:IntervalSeconds"] = _options.ScheduledPostPublisherIntervalSeconds.ToString(),
                ["BackgroundJobs:NotificationDelivery:Enabled"] = _options.BackgroundJobsEnabled ? "true" : "false",
                ["BackgroundJobs:NotificationDelivery:IntervalSeconds"] = _options.NotificationDeliveryIntervalSeconds.ToString(),
                ["BackgroundJobs:ExpiredTokenCleanup:Enabled"] = _options.BackgroundJobsEnabled ? "true" : "false",
                ["BackgroundJobs:ExpiredTokenCleanup:IntervalMinutes"] = _options.ExpiredTokenCleanupIntervalMinutes.ToString(),
                ["Outbox:Enabled"] = _options.OutboxEnabled ? "true" : "false",
                ["Outbox:IntervalSeconds"] = _options.OutboxIntervalSeconds.ToString(),
                ["Outbox:BatchSize"] = "50",
                ["Outbox:RetryDelaySeconds"] = "1",
                ["Outbox:MaxRetries"] = "3"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ContentHubDbContext>>();
            services.RemoveAll<IAuthEmailSender>();

            services.AddDbContext<ContentHubDbContext>(options =>
            {
                options.UseNpgsql(_databaseFixture.ConnectionString);
            });

            services.AddSingleton<TestAuthEmailSender>();
            services.AddSingleton<IAuthEmailSender>(sp =>
                sp.GetRequiredService<TestAuthEmailSender>());
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

        await db.Database.MigrateAsync();

        var seeder = new TestDataSeeder(this);

        await seeder.SeedRolesAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
