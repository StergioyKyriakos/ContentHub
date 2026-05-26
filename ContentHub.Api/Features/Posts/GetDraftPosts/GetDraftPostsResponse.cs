using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.GetDraftPosts;

public sealed class GetDraftPostsResponse
{
    public IReadOnlyList<PostListItemDto> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}