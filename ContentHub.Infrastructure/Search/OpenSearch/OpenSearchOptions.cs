namespace ContentHub.Infrastructure.Search.OpenSearch;

public sealed class OpenSearchOptions
{
    public const string SectionName = "Search";

    public string Provider { get; set; } = "PostgreSql";

    public string Endpoint { get; set; } = "http://localhost:9200";

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string IndexPrefix { get; set; } = "contenthub";

    public int RequestTimeoutSeconds { get; set; } = 30;

    public bool UseOpenSearch()
    {
        return string.Equals(Provider, "OpenSearch", StringComparison.OrdinalIgnoreCase);
    }

    public string PostsIndexName()
    {
        return $"{IndexPrefix.Trim().ToLowerInvariant()}-posts";
    }

    public string DocumentsIndexName()
    {
        return $"{IndexPrefix.Trim().ToLowerInvariant()}-documents";
    }
}
