using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.UnpublishPost;

public sealed class UnpublishPostResponse
{
    public Guid Id { get; set; }

    public PostStatusDto Status { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
}