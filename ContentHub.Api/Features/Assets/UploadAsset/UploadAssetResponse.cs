using ContentHub.Data.Dtos.Assets;

namespace ContentHub.Api.Features.Assets.UploadAsset;

public sealed class UploadAssetResponse
{
    public AssetDto Asset { get; set; } = null!;
}