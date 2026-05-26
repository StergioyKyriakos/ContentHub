using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.SetFeaturedPost;

public sealed class SetFeaturedPostResponse
{
    public Guid Id { get; set; }

    public PostStatusDto Status { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
}