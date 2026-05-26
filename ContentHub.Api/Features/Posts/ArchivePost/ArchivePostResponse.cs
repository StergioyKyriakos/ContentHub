using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.ArchivePost;

public sealed class ArchivePostResponse
{
    public Guid Id { get; set; }

    public PostStatusDto Status { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
}