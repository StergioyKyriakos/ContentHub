using ContentHub.Data.Dtos.Assets;
using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.Assets.GetAssets;

public sealed class GetAssetsResponse
{
    public PagedResponse<AssetSummaryDto> Data { get; set; } = null!;
}