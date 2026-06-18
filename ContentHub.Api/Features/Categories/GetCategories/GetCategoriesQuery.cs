namespace ContentHub.Api.Features.Categories.GetCategories;

public sealed class GetCategoriesQuery
{
    public Guid? ParentCategoryId { get; set; }

    public bool IncludeHidden { get; set; } = false;

    public string? Search { get; set; }
}
