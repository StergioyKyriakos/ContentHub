using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Authors.GetAuthorPosts;

public sealed class GetAuthorPostsResponse
{
    public Guid AuthorId { get; set; }

    public IReadOnlyCollection<PostSummaryDto> Posts { get; set; } = [];
}
