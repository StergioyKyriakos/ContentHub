namespace ContentHub.Infrastructure.Caching;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";

    public bool Enabled { get; set; } = true;

    public int FeaturedPostsCacheMinutes { get; set; } = 5;
}
