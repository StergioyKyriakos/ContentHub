using ContentHub.Data.Enums;

namespace ContentHub.Api.Features.Assets.GetAssets;

public sealed class GetAssetsQuery
{
    public AssetType? Type { get; set; }

    public AssetVisibility? Visibility { get; set; }

    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}