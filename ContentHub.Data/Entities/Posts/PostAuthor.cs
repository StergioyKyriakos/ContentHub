using ContentHub.Data.Entities.Authors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Posts;

public sealed class PostAuthor
{
    public PostAuthor()
    {
    }

    public PostAuthor(Guid postId, Guid authorId)
    {
        PostId = postId;
        AuthorId = authorId;
    }

    public Guid PostId { get; set; }

    public Guid AuthorId { get; set; }

    public Post Post { get; set; } = null!;

    public Author Author { get; set; } = null!;
}

public sealed class PostAuthorConfiguration : IEntityTypeConfiguration<PostAuthor>
{
    public void Configure(EntityTypeBuilder<PostAuthor> builder)
    {
        builder.ToTable("post_authors");

        builder.HasKey(postAuthor => new
        {
            postAuthor.PostId,
            postAuthor.AuthorId
        });

        builder.HasOne(postAuthor => postAuthor.Post)
            .WithMany(post => post.Authors)
            .HasForeignKey(postAuthor => postAuthor.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(postAuthor => postAuthor.Author)
            .WithMany()
            .HasForeignKey(postAuthor => postAuthor.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(postAuthor => postAuthor.AuthorId);
    }
}