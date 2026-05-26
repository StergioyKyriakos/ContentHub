using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.GetFeaturedPosts;

public sealed class GetFeaturedPostsResponse
{
    public PagedResponse<PostSummaryDto> Posts { get; set; } = null!;
}