using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.RemoveFeaturedPost;

public sealed class RemoveFeaturedPostResponse
{
    public Guid Id { get; set; }

    public PostStatusDto Status { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
}