using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Search.SearchPosts;

public sealed class SearchPostsQuery
{
    public string? Q { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? AuthorId { get; set; }

    public PostStatusDto? Status { get; set; }

    public DateTime? PublishedFrom { get; set; }

    public DateTime? PublishedTo { get; set; }

    public bool? IsFeatured { get; set; }

    public string? SortBy { get; set; } = "publishedAt";

    public string? SortDirection { get; set; } = "desc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public bool IncludeUnpublished { get; set; } = false;
}
