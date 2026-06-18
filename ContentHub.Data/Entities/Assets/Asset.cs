using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace ContentHub.Data.Entities.Assets;

public sealed class Asset : AggregateRoot
{
    private readonly List<AssetVersion> _versions = [];

    public Asset()
    {
    }

    public Asset(
        string fileName,
        string originalFileName,
        string contentType,
        long size,
        string hash,
        string storagePath,
        StorageProvider provider,
        AssetVisibility visibility,
        AssetType type,
        Guid uploadedById)
    {
        FileName = fileName;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        Size = size;
        Hash = hash;
        StoragePath = storagePath;
        Provider = provider;
        Visibility = visibility;
        Type = type;
        UploadedById = uploadedById;
    }

    public string FileName { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public string Hash { get; set; } = null!;

    public string StoragePath { get; set; } = null!;

    public StorageProvider Provider { get; set; }

    public AssetVisibility Visibility { get; set; }

    public AssetType Type { get; set; }

    public Guid UploadedById { get; set; }

    public NpgsqlTsVector SearchVector { get; private set; } = null!;

    public IReadOnlyCollection<AssetVersion> Versions => _versions.AsReadOnly();

    public void ChangeVisibility(AssetVisibility visibility)
    {
        Visibility = visibility;
        MarkAsUpdated();
    }

    public void AddVersion(
        string fileName,
        string contentType,
        long size,
        string hash,
        string storagePath,
        int versionNumber)
    {
        _versions.Add(new AssetVersion(
            assetId: Id,
            fileName: fileName,
            contentType: contentType,
            size: size,
            hash: hash,
            storagePath: storagePath,
            versionNumber: versionNumber));

        MarkAsUpdated();
    }
}

public sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("assets");

        builder.HasKey(asset => asset.Id);

        builder.Property(asset => asset.Id)
            .ValueGeneratedNever();

        builder.Property(asset => asset.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(asset => asset.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(asset => asset.ContentType)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(asset => asset.Size)
            .IsRequired();

        builder.Property(asset => asset.Hash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(asset => asset.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(asset => asset.Provider)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(asset => asset.Visibility)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(asset => asset.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(asset => asset.UploadedById)
            .IsRequired();

        builder.Property(asset => asset.SearchVector)
            .HasColumnType("tsvector")
            .HasComputedColumnSql(
                """
                setweight(to_tsvector('simple', coalesce("OriginalFileName", '')), 'A') ||
                setweight(to_tsvector('simple', coalesce("FileName", '')), 'B') ||
                setweight(to_tsvector('simple', coalesce("ContentType", '')), 'B') ||
                setweight(to_tsvector('simple', coalesce("StoragePath", '')), 'C')
                """,
                stored: true);

        builder.Property(asset => asset.CreatedAtUtc)
            .IsRequired();

        builder.Property(asset => asset.UpdatedAtUtc);

        builder.Property(asset => asset.DeletedAtUtc);

        builder.Property(asset => asset.IsDeleted)
            .IsRequired();

        builder.HasIndex(asset => asset.Hash);

        builder.HasIndex(asset => asset.UploadedById);

        builder.HasIndex(asset => asset.Type);

        builder.HasIndex(asset => asset.Visibility);

        builder.HasIndex(asset => asset.SearchVector)
            .HasMethod("GIN");

        builder.HasQueryFilter(asset => !asset.IsDeleted);

        builder.HasMany(asset => asset.Versions)
            .WithOne(version => version.Asset)
            .HasForeignKey(version => version.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
