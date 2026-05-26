using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.GetPosts;

public sealed class GetPostsQuery
{
    public string? Search { get; set; }

    public PostStatusDto? Status { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? AuthorId { get; set; }

    public bool? IsFeatured { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}