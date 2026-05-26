using ContentHub.Data.Enums;

namespace ContentHub.Data.Dtos.Assets;

public sealed class AssetSummaryDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public string Url { get; set; } = null!;

    public AssetType Type { get; set; }

    public AssetVisibility Visibility { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}