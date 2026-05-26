namespace ContentHub.Api.Features.Posts.CreatePost;

public sealed class CreatePostCommand
{
    public string Title { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string Content { get; set; } = null!;

    public Guid? CoverAssetId { get; set; }

    public IReadOnlyCollection<Guid> CategoryIds { get; set; } = [];

    public IReadOnlyCollection<Guid> AuthorIds { get; set; } = [];

    public IReadOnlyCollection<string> Tags { get; set; } = [];
}