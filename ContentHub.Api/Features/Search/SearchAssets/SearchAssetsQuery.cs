using ContentHub.Data.Enums;

namespace ContentHub.Api.Features.Search.SearchAssets;

public sealed class SearchAssetsQuery
{
    public string? Q { get; set; }

    public AssetType? Type { get; set; }

    public AssetVisibility? Visibility { get; set; }

    public string? ContentType { get; set; }

    public string? SortBy { get; set; } = "createdAt";

    public string? SortDirection { get; set; } = "desc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public static ValueTask<SearchAssetsQuery> BindAsync(HttpContext context)
    {
        var query = context.Request.Query;

        return ValueTask.FromResult(new SearchAssetsQuery
        {
            Q = GetString(query, "q"),
            Type = GetEnum<AssetType>(query, "type"),
            Visibility = GetEnum<AssetVisibility>(query, "visibility"),
            ContentType = GetString(query, "contentType"),
            SortBy = GetString(query, "sortBy") ?? "createdAt",
            SortDirection = GetString(query, "sortDirection") ?? "desc",
            Page = GetInt(query, "page") ?? 1,
            PageSize = GetInt(query, "pageSize") ?? 20
        });
    }

    private static string? GetString(IQueryCollection query, string key) =>
        query.TryGetValue(key, out var value) ? value.ToString() : null;

    private static int? GetInt(IQueryCollection query, string key) =>
        int.TryParse(GetString(query, key), out var value) ? value : null;

    private static TEnum? GetEnum<TEnum>(IQueryCollection query, string key)
        where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(GetString(query, key), ignoreCase: true, out var value)
            ? value
            : null;
}
