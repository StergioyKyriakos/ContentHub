using ContentHub.Data.Dtos.Assets;
using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.Search.SearchAssets;

public sealed class SearchAssetsResponse
{
    public PagedResponse<AssetSummaryDto> Assets { get; set; } = null!;
}