namespace ContentHub.Api.Features.Search.SearchEverything;

public sealed class SearchEverythingQuery
{
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public static ValueTask<SearchEverythingQuery> BindAsync(HttpContext context)
    {
        var query = context.Request.Query;

        return ValueTask.FromResult(new SearchEverythingQuery
        {
            Q = GetString(query, "q"),
            Page = GetInt(query, "page") ?? 1,
            PageSize = GetInt(query, "pageSize") ?? 20
        });
    }

    private static string? GetString(IQueryCollection query, string key) =>
        query.TryGetValue(key, out var value) ? value.ToString() : null;

    private static int? GetInt(IQueryCollection query, string key) =>
        int.TryParse(GetString(query, key), out var value) ? value : null;
}
