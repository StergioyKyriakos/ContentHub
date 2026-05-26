namespace ContentHub.Api.Features.Categories.GetCategories;

public sealed class GetCategoriesQuery
{
    public Guid? ParentCategoryId { get; set; }

    public bool IncludeHidden { get; set; } = false;

    public string? Search { get; set; }

    public static ValueTask<GetCategoriesQuery> BindAsync(HttpContext context)
    {
        var query = context.Request.Query;

        return ValueTask.FromResult(new GetCategoriesQuery
        {
            ParentCategoryId = GetGuid(query, "parentCategoryId"),
            IncludeHidden = GetBool(query, "includeHidden") ?? false,
            Search = GetString(query, "search")
        });
    }

    private static string? GetString(IQueryCollection query, string key) =>
        query.TryGetValue(key, out var value) ? value.ToString() : null;

    private static Guid? GetGuid(IQueryCollection query, string key) =>
        Guid.TryParse(GetString(query, key), out var value) ? value : null;

    private static bool? GetBool(IQueryCollection query, string key) =>
        bool.TryParse(GetString(query, key), out var value) ? value : null;
}
