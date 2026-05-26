using ContentHub.Data.Enums;

namespace ContentHub.Data.Dtos.Search;

public sealed class SearchEverythingItemDto
{
    public SearchableContentType Type { get; set; }

    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Slug { get; set; }

    public string? Url { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}