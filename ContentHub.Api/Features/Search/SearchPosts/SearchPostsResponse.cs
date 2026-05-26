using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Search.SearchPosts;

public sealed class SearchPostsResponse
{
    public IReadOnlyList<PostSummaryDto> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}