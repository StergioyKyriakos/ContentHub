using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.UpdatePost;

public class UpdatePostResponse
{
    public PostDto Post { get; set; } = null!;
}