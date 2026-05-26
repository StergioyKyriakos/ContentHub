namespace ContentHub.Data.Dtos.Categories;

public sealed class CategorySummaryDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Slug { get; set; } 

    public Guid? ParentCategoryId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; }
}