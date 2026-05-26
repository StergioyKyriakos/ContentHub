namespace ContentHub.Api.Features.Authors.GetAuthorPosts;

public sealed class GetAuthorPostsResponse
{
    public Guid AuthorId { get; set; }

    public IReadOnlyCollection<object> Posts { get; set; } = [];
}