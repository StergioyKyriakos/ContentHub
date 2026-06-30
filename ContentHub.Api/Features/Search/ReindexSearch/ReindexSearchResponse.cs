namespace ContentHub.Api.Features.Search.ReindexSearch;

public sealed class ReindexSearchResponse
{
    public string Provider { get; set; } = string.Empty;

    public bool OpenSearchEnabled { get; set; }

    public int PostsIndexed { get; set; }

    public int DocumentsIndexed { get; set; }

    public string Message { get; set; } = string.Empty;
}
