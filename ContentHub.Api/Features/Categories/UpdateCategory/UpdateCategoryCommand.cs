namespace ContentHub.Api.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryCommand
{
    public required string Name { get; set; }

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; }
}