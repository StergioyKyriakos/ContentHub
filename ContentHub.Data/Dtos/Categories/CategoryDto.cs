namespace ContentHub.Data.Dtos.Categories;

public sealed class CategoryDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; } 

    public required string Slug { get; set; }

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public string? ParentCategoryName { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}