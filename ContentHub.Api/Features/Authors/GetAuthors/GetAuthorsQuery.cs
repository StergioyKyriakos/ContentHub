namespace ContentHub.Api.Features.Authors.GetAuthors;

public sealed class GetAuthorsQuery
{
    public bool IncludeInactive { get; set; } = false;

    public string? Search { get; set; }

    public static ValueTask<GetAuthorsQuery> BindAsync(HttpContext context)
    {
        var query = context.Request.Query;

        return ValueTask.FromResult(new GetAuthorsQuery
        {
            IncludeInactive = GetBool(query, "includeInactive") ?? false,
            Search = GetString(query, "search")
        });
    }

    private static string? GetString(IQueryCollection query, string key) =>
        query.TryGetValue(key, out var value) ? value.ToString() : null;

    private static bool? GetBool(IQueryCollection query, string key) =>
        bool.TryParse(GetString(query, key), out var value) ? value : null;
}
