using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.PublishPost;

public sealed class PublishPostResponse
{
    public Guid Id { get; set; }

    public PostStatusDto Status { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
}