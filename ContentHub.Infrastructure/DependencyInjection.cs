using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Abstractions.Caching;
using ContentHub.Application.Abstractions.RateLimiting;
using ContentHub.Infrastructure.Authentication;
using ContentHub.Infrastructure.BackgroundJobs;
using ContentHub.Infrastructure.Caching;
using ContentHub.Infrastructure.Outbox;
using ContentHub.Infrastructure.RateLimiting;
using ContentHub.Infrastructure.Search.OpenSearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ContentHub.Application.Abstractions.Storage;
using ContentHub.Infrastructure.Storage;
using ContentHub.Infrastructure.Storage.Cloud;
using ContentHub.Infrastructure.Storage.Local;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddContentHubInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AuthLinkOptions>(
            configuration.GetSection(AuthLinkOptions.SectionName));
        services.Configure<SmtpEmailOptions>(
            configuration.GetSection(SmtpEmailOptions.SectionName));
        services.Configure<RedisOptions>(
            configuration.GetSection(RedisOptions.SectionName));
        services.Configure<RateLimitOptions>(
            configuration.GetSection(RateLimitOptions.SectionName));
        services.Configure<BackgroundJobOptions>(
            configuration.GetSection(BackgroundJobOptions.SectionName));
        services.Configure<OutboxOptions>(
            configuration.GetSection(OutboxOptions.SectionName));
        services.Configure<OpenSearchOptions>(
            configuration.GetSection(OpenSearchOptions.SectionName));
        services.Configure<StorageOptions>(
            configuration.GetSection(StorageOptions.SectionName));
        services.Configure<AzureBlobStorageOptions>(
            configuration.GetSection(AzureBlobStorageOptions.SectionName));
        services.Configure<S3StorageOptions>(
            configuration.GetSection(S3StorageOptions.SectionName));

        services.AddHttpContextAccessor();

        services.AddSingleton<RedisConnectionFactory>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IRateLimitService, RedisRateLimitService>();
        services.AddSingleton<CacheInvalidationService>();
        services.AddSingleton<RateLimitKeyBuilder>();
        services.AddScoped<OpenSearchIndex>();

        services.AddHostedService<ScheduledPostPublisherJob>();
        services.AddHostedService<NotificationDeliveryJob>();
        services.AddHostedService<ExpiredTokenCleanupJob>();
        services.AddSingleton<OutboxMessageProcessor>();
        services.AddHostedService(sp => sp.GetRequiredService<OutboxMessageProcessor>());
        services.AddScoped<ExpiredTokenCleanupService>();
        services.AddSingleton<BackgroundJobLockService>();

        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<ISecurityTokenGenerator, SecurityTokenGenerator>();
        if (configuration.GetValue<bool>($"{SmtpEmailOptions.SectionName}:Enabled"))
        {
            services.AddScoped<IAuthEmailSender, SmtpAuthEmailSender>();
        }
        else
        {
            services.AddScoped<IAuthEmailSender, DevelopmentAuthEmailSender>();
        }
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<IFileHashCalculator, FileHashCalculator>();
        services.AddScoped<IFileUrlResolver, FileUrlResolver>();
        services.AddScoped<LocalFileStorage>();
        services.AddScoped<AzureBlobFileStorage>();
        services.AddScoped<S3FileStorage>();
        services.AddScoped<IFileStorageFactory, FileStorageFactory>();
        services.AddScoped<IFileStorage>(sp =>
            sp.GetRequiredService<IFileStorageFactory>().GetCurrent());
        services.AddScoped<ITwoFactorService, TotpTwoFactorService>();

        return services;
    }
}
