using ContentHub.Data.Persistence;
using ContentHub.Data.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContentHub.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddContentHubData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Database' was not configured.");
        }

        services.AddDbContext<ContentHubDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        
        services.AddScoped<RoleSeeder>();
        services.AddScoped<AdminUserSeeder>();
        services.AddScoped<ContentSeeder>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
