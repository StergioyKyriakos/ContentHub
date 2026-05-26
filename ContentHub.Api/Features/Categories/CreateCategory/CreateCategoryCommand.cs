namespace ContentHub.Api.Features.Categories.CreateCategory;

public sealed class CreateCategoryCommand
{
    public required string Name { get; set; }

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; } = true;
}