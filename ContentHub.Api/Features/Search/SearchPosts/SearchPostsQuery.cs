using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Search.SearchPosts;

public sealed class SearchPostsQuery
{
    public string? Q { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? AuthorId { get; set; }

    public PostStatusDto? Status { get; set; }

    public DateTime? PublishedFrom { get; set; }

    public DateTime? PublishedTo { get; set; }

    public bool? IsFeatured { get; set; }

    public string? SortBy { get; set; } = "publishedAt";

    public string? SortDirection { get; set; } = "desc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public bool IncludeUnpublished { get; set; } = false;

    public static ValueTask<SearchPostsQuery> BindAsync(HttpContext context)
    {
        var query = context.Request.Query;

        return ValueTask.FromResult(new SearchPostsQuery
        {
            Q = GetString(query, "q"),
            CategoryId = GetGuid(query, "categoryId"),
            AuthorId = GetGuid(query, "authorId"),
            Status = GetEnum<PostStatusDto>(query, "status"),
            PublishedFrom = GetDateTime(query, "publishedFrom"),
            PublishedTo = GetDateTime(query, "publishedTo"),
            IsFeatured = GetBool(query, "isFeatured"),
            SortBy = GetString(query, "sortBy") ?? "publishedAt",
            SortDirection = GetString(query, "sortDirection") ?? "desc",
            Page = GetInt(query, "page") ?? 1,
            PageSize = GetInt(query, "pageSize") ?? 20,
            IncludeUnpublished = GetBool(query, "includeUnpublished") ?? false
        });
    }

    private static string? GetString(IQueryCollection query, string key) =>
        query.TryGetValue(key, out var value) ? value.ToString() : null;

    private static int? GetInt(IQueryCollection query, string key) =>
        int.TryParse(GetString(query, key), out var value) ? value : null;

    private static Guid? GetGuid(IQueryCollection query, string key) =>
        Guid.TryParse(GetString(query, key), out var value) ? value : null;

    private static bool? GetBool(IQueryCollection query, string key) =>
        bool.TryParse(GetString(query, key), out var value) ? value : null;

    private static DateTime? GetDateTime(IQueryCollection query, string key) =>
        DateTime.TryParse(GetString(query, key), out var value) ? value : null;

    private static TEnum? GetEnum<TEnum>(IQueryCollection query, string key)
        where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(GetString(query, key), ignoreCase: true, out var value)
            ? value
            : null;
}
