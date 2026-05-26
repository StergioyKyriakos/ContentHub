using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Posts;

public sealed class PostTag
{
    public PostTag()
    {
    }

    public PostTag(Guid postId, string name)
    {
        PostId = postId;
        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
    }

    public Guid PostId { get; set; }

    public string Name { get; set; } = null!;

    public string NormalizedName { get; set; } = null!;

    public Post Post { get; set; } = null!;
}

public sealed class PostTagConfiguration : IEntityTypeConfiguration<PostTag>
{
    public void Configure(EntityTypeBuilder<PostTag> builder)
    {
        builder.ToTable("post_tags");

        builder.HasKey(postTag => new
        {
            postTag.PostId,
            postTag.NormalizedName
        });

        builder.Property(postTag => postTag.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(postTag => postTag.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(postTag => postTag.Post)
            .WithMany(post => post.Tags)
            .HasForeignKey(postTag => postTag.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(postTag => postTag.NormalizedName);
    }
}