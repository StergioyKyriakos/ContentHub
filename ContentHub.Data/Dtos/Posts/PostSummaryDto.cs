namespace ContentHub.Data.Dtos.Posts;

public sealed class PostSummaryDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public Guid? CoverAssetId { get; set; }
}