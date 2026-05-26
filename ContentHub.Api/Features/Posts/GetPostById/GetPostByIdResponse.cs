using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.GetPostById;

public sealed class GetPostByIdResponse
{
    public PostDto Post { get; set; } = null!;
}