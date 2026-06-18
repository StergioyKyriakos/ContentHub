namespace ContentHub.Api.Features.Search.SearchEverything;

public sealed class SearchEverythingQuery
{
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
