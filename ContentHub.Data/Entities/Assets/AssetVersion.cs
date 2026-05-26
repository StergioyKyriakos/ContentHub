using ContentHub.Data.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Assets;

public sealed class AssetVersion : Entity
{
    public AssetVersion()
    {
    }

    public AssetVersion(
        Guid assetId,
        string fileName,
        string contentType,
        long size,
        string hash,
        string storagePath,
        int versionNumber)
    {
        AssetId = assetId;
        FileName = fileName;
        ContentType = contentType;
        Size = size;
        Hash = hash;
        StoragePath = storagePath;
        VersionNumber = versionNumber;
    }

    public Guid AssetId { get; set; }

    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public string Hash { get; set; } = null!;

    public string StoragePath { get; set; } = null!;

    public int VersionNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Asset Asset { get; set; } = null!;
}

public sealed class AssetVersionConfiguration : IEntityTypeConfiguration<AssetVersion>
{
    public void Configure(EntityTypeBuilder<AssetVersion> builder)
    {
        builder.ToTable("asset_versions");

        builder.HasKey(version => version.Id);

        builder.Property(version => version.Id)
            .ValueGeneratedNever();

        builder.Property(version => version.AssetId)
            .IsRequired();

        builder.Property(version => version.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(version => version.ContentType)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(version => version.Size)
            .IsRequired();

        builder.Property(version => version.Hash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(version => version.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(version => version.VersionNumber)
            .IsRequired();

        builder.Property(version => version.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(version => version.AssetId);

        builder.HasIndex(version => new
        {
            version.AssetId,
            version.VersionNumber
        }).IsUnique();
    }
}