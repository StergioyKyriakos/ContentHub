using ContentHub.Data.Dtos.Assets;

namespace ContentHub.Api.Features.Assets.GetAssetById;

public sealed class GetAssetByIdResponse
{
    public AssetDto Asset { get; set; } = null!;
}