using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Search;

namespace ContentHub.Api.Features.Search.SearchEverything;

public sealed class SearchEverythingResponse
{
    public PagedResponse<SearchEverythingItemDto> Results { get; set; } = null!;
}

