using ContentHub.Data.Enums;

namespace ContentHub.Infrastructure.Search.OpenSearch;

public sealed class OpenSearchPostDocument
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string Content { get; set; } = string.Empty;

    public PostStatus Status { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Guid? CoverAssetId { get; set; }

    public IReadOnlyCollection<Guid> CategoryIds { get; set; } = [];

    public IReadOnlyCollection<Guid> AuthorIds { get; set; } = [];

    public IReadOnlyCollection<string> Tags { get; set; } = [];

    public string Url { get; set; } = string.Empty;
}

public sealed class OpenSearchDocumentDocument
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public string StoragePath { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public AssetType Type { get; set; }

    public AssetVisibility Visibility { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}

public sealed class OpenSearchSearchResult<T>
{
    public IReadOnlyList<OpenSearchSearchHit<T>> Items { get; set; } = [];

    public int TotalCount { get; set; }
}

public sealed class OpenSearchSearchHit<T>
{
    public T Document { get; set; } = default!;

    public IReadOnlyCollection<string> Highlights { get; set; } = [];
}

public sealed class OpenSearchPostSearchRequest
{
    public string? Query { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? AuthorId { get; set; }

    public PostStatus? Status { get; set; }

    public DateTime? PublishedFrom { get; set; }

    public DateTime? PublishedTo { get; set; }

    public bool? IsFeatured { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public bool IncludeUnpublished { get; set; }
}

public sealed class OpenSearchDocumentSearchRequest
{
    public string? Query { get; set; }

    public AssetVisibility? Visibility { get; set; }

    public string? ContentType { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public sealed class OpenSearchReindexResult
{
    public string Provider { get; set; } = "PostgreSql";

    public bool OpenSearchEnabled { get; set; }

    public int PostsIndexed { get; set; }

    public int DocumentsIndexed { get; set; }

    public string Message { get; set; } = string.Empty;
}
