using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Posts;

public sealed class PostAsset
{
    public PostAsset()
    {
    }

    public PostAsset(Guid postId, Guid assetId, int displayOrder)
    {
        PostId = postId;
        AssetId = assetId;
        DisplayOrder = displayOrder;
    }

    public Guid PostId { get; set; }

    public Guid AssetId { get; set; }

    public int DisplayOrder { get; set; }

    public Post Post { get; set; } = null!;
}

public sealed class PostAssetConfiguration : IEntityTypeConfiguration<PostAsset>
{
    public void Configure(EntityTypeBuilder<PostAsset> builder)
    {
        builder.ToTable("post_assets");

        builder.HasKey(postAsset => new
        {
            postAsset.PostId,
            postAsset.AssetId
        });

        builder.Property(postAsset => postAsset.DisplayOrder)
            .IsRequired();

        builder.HasOne(postAsset => postAsset.Post)
            .WithMany(post => post.Assets)
            .HasForeignKey(postAsset => postAsset.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(postAsset => postAsset.AssetId);
    }
}