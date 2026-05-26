using ContentHub.Data.Enums;

namespace ContentHub.Data.Dtos.Assets;

public sealed class AssetDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public string Hash { get; set; } = null!;

    public string StoragePath { get; set; } = null!;

    public string Url { get; set; } = null!;

    public StorageProvider Provider { get; set; }

    public AssetVisibility Visibility { get; set; }

    public AssetType Type { get; set; }

    public Guid UploadedById { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}