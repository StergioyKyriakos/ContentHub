using ContentHub.Api.Common.Auditing;

namespace ContentHub.Api.Extensions;

public static class AuditingExtensions
{
    public static IServiceCollection AddContentHubAuditing(
        this IServiceCollection services)
    {
        services.AddScoped<AuditLogWriter>();

        return services;
    }
}