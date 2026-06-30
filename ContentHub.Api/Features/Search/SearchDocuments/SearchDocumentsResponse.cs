using ContentHub.Data.Dtos.Assets;
using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.Search.SearchDocuments;

public sealed class SearchDocumentsResponse
{
    public PagedResponse<AssetSummaryDto> Documents { get; set; } = null!;
}
