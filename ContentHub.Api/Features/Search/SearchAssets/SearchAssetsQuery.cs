using ContentHub.Data.Enums;

namespace ContentHub.Api.Features.Search.SearchAssets;

public sealed class SearchAssetsQuery
{
    public string? Q { get; set; }

    public AssetType? Type { get; set; }

    public AssetVisibility? Visibility { get; set; }

    public string? ContentType { get; set; }

    public string? SortBy { get; set; } = "createdAt";

    public string? SortDirection { get; set; } = "desc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
