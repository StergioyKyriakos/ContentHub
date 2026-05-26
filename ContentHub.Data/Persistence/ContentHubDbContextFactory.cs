using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ContentHub.Data.Persistence;

public sealed class ContentHubDbContextFactory : IDesignTimeDbContextFactory<ContentHubDbContext>
{
    public ContentHubDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        var apiProjectPath = Path.Combine(
            currentDirectory,
            "..",
            "ContentHub.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Database");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Database' was not found.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ContentHubDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new ContentHubDbContext(optionsBuilder.Options);
    }
}