using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.CreatePost;

public class CreatePostResponse
{
    public PostDto? Post { get; set; } = null!;
}