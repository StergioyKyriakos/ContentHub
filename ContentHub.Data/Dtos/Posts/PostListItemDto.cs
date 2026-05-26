namespace ContentHub.Data.Dtos.Posts;

public sealed class PostListItemDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public PostStatusDto Status { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public DateTime? ScheduledForUtc { get; set; }

    public Guid? CoverAssetId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}