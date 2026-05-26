using ContentHub.Data.Enums;

namespace ContentHub.Api.Features.Assets.UploadAsset;

public sealed class UploadAssetCommand
{
    public IFormFile File { get; set; } = null!;

    public AssetVisibility Visibility { get; set; } = AssetVisibility.Public;
}