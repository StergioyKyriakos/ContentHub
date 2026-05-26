using ContentHub.Data.Entities.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Posts;

public sealed class PostCategory
{
    private PostCategory()
    {
    }

    public PostCategory(Guid postId, Guid categoryId)
    {
        PostId = postId;
        CategoryId = categoryId;
    }

    public Guid PostId { get; set; }

    public Guid CategoryId { get; set; }

    public Post Post { get; set; } = null!;

    public Category Category { get; set; } = null!;
}

public sealed class PostCategoryConfiguration : IEntityTypeConfiguration<PostCategory>
{
    public void Configure(EntityTypeBuilder<PostCategory> builder)
    {
        builder.ToTable("post_categories");

        builder.HasKey(postCategory => new
        {
            postCategory.PostId,
            postCategory.CategoryId
        });

        builder.HasOne(postCategory => postCategory.Post)
            .WithMany(post => post.Categories)
            .HasForeignKey(postCategory => postCategory.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(postCategory => postCategory.Category)
            .WithMany()
            .HasForeignKey(postCategory => postCategory.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(postCategory => postCategory.CategoryId);
    }
}