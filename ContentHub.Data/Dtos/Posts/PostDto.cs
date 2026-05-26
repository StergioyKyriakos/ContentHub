using ContentHub.Data.Dtos.Authors;
using ContentHub.Data.Dtos.Categories;

namespace ContentHub.Data.Dtos.Posts;

public sealed class PostDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public string Content { get; set; } = null!;

    public PostStatusDto Status { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime? FeaturedAtUtc { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public DateTime? ScheduledForUtc { get; set; }

    public Guid? CoverAssetId { get; set; }

    public Guid CreatedById { get; set; }

    public Guid? UpdatedById { get; set; }

    public IReadOnlyCollection<CategorySummaryDto> Categories { get; set; } = [];

    public IReadOnlyCollection<AuthorSummaryDto> Authors { get; set; } = [];

    public IReadOnlyCollection<string> Tags { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}