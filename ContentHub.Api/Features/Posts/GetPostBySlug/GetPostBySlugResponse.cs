using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.GetPostBySlug;

public sealed class GetPostBySlugResponse
{
    public PostDto Post { get; set; } = null!;
}