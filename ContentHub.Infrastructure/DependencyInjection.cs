using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ContentHub.Application.Abstractions.Storage;
using ContentHub.Infrastructure.Storage;
using ContentHub.Infrastructure.Storage.Local;

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

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<ISecurityTokenGenerator, SecurityTokenGenerator>();
        services.AddScoped<IAuthEmailSender, DevelopmentAuthEmailSender>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<IFileHashCalculator, FileHashCalculator>();
        services.AddScoped<IFileUrlResolver, FileUrlResolver>();
        services.AddScoped<IFileStorage, LocalFileStorage>();

        return services;
    }
}
