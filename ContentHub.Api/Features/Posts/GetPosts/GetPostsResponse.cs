using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.GetPosts;

public sealed class GetPostsResponse
{
    public PagedResponse<PostListItemDto> Posts { get; set; } = null!;
}