using ContentHub.Data.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Categories;

public sealed class Category : AggregateRoot
{
    private readonly List<Category> _children = [];

    public Category()
    {
    }

    public Category(
        string name,
        string slug,
        string? description,
        Guid? parentCategoryId,
        int displayOrder,
        bool isVisible)
    {
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        ParentCategoryId = parentCategoryId;
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
    }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public Category? ParentCategory { get; set; }

    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; }

    public void Update(
        string name,
        string slug,
        string? description,
        Guid? parentCategoryId,
        int displayOrder,
        bool isVisible)
    {
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        ParentCategoryId = parentCategoryId;
        DisplayOrder = displayOrder;
        IsVisible = isVisible;

        MarkAsUpdated();
    }

    public void Hide()
    {
        IsVisible = false;
        MarkAsUpdated();
    }

    public void Show()
    {
        IsVisible = true;
        MarkAsUpdated();
    }
}

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .ValueGeneratedNever();

        builder.Property(category => category.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(category => category.Slug)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(category => category.Description)
            .HasMaxLength(1000);

        builder.Property(category => category.ParentCategoryId);

        builder.Property(category => category.DisplayOrder)
            .IsRequired();

        builder.Property(category => category.IsVisible)
            .IsRequired();

        builder.Property(category => category.CreatedAtUtc)
            .IsRequired();

        builder.Property(category => category.UpdatedAtUtc);

        builder.Property(category => category.DeletedAtUtc);

        builder.Property(category => category.IsDeleted)
            .IsRequired();

        builder.HasIndex(category => category.Slug)
            .IsUnique();

        builder.HasIndex(category => category.ParentCategoryId);

        builder.HasIndex(category => category.DisplayOrder);

        builder.HasOne(category => category.ParentCategory)
            .WithMany(category => category.Children)
            .HasForeignKey(category => category.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(category => !category.IsDeleted);
    }
}