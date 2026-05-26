namespace ContentHub.Api.Features.Posts.GetPublishedPosts;

public sealed class GetPublishedPostsQuery
{
    public string? Search { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? AuthorId { get; set; }

    public bool? IsFeatured { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}