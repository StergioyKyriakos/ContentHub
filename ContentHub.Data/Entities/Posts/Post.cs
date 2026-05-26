using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Posts;

public sealed class Post : AggregateRoot
{
    private readonly List<PostCategory> _categories = [];
    private readonly List<PostAuthor> _authors = [];
    private readonly List<PostAsset> _assets = [];
    private readonly List<PostTag> _tags = [];

    public Post()
    {
    }

    public Post(
        string title,
        string slug,
        string? summary,
        string content,
        Guid createdById,
        Guid? coverAssetId = null)
    {
        Title = title.Trim();
        Slug = slug.Trim().ToLowerInvariant();

        Summary = string.IsNullOrWhiteSpace(summary)
            ? null
            : summary.Trim();

        Content = content.Trim();
        Status = PostStatus.Draft;

        CreatedById = createdById;
        CoverAssetId = coverAssetId;

        IsFeatured = false;
    }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public string Content { get; set; } = null!;

    public PostStatus Status { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime? FeaturedAtUtc { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public DateTime? ScheduledForUtc { get; set; }

    public Guid? CoverAssetId { get; set; }

    public Guid CreatedById { get; set; }

    public Guid? UpdatedById { get; set; }

    public IReadOnlyCollection<PostCategory> Categories => _categories.AsReadOnly();

    public IReadOnlyCollection<PostAuthor> Authors => _authors.AsReadOnly();

    public IReadOnlyCollection<PostAsset> Assets => _assets.AsReadOnly();

    public IReadOnlyCollection<PostTag> Tags => _tags.AsReadOnly();

    public bool IsDraft => Status == PostStatus.Draft;

    public bool IsPublished => Status == PostStatus.Published;

    public bool IsScheduled => Status == PostStatus.Scheduled;

    public bool IsArchived => Status == PostStatus.Archived;

    public void Update(
        string title,
        string slug,
        string? summary,
        string content,
        Guid? coverAssetId,
        Guid updatedById)
    {
        Title = title.Trim();
        Slug = slug.Trim().ToLowerInvariant();

        Summary = string.IsNullOrWhiteSpace(summary)
            ? null
            : summary.Trim();

        Content = content.Trim();
        CoverAssetId = coverAssetId;
        UpdatedById = updatedById;

        MarkAsUpdated();
    }

    public void ReplaceCategories(IEnumerable<Guid> categoryIds)
    {
        _categories.Clear();

        foreach (var categoryId in categoryIds.Distinct())
        {
            _categories.Add(new PostCategory(Id, categoryId));
        }

        MarkAsUpdated();
    }

    public void ReplaceAuthors(IEnumerable<Guid> authorIds)
    {
        _authors.Clear();

        foreach (var authorId in authorIds.Distinct())
        {
            _authors.Add(new PostAuthor(Id, authorId));
        }

        MarkAsUpdated();
    }

    public void ReplaceTags(IEnumerable<string> tags)
    {
        _tags.Clear();

        foreach (var tag in tags.Select(tag => tag.Trim()).Where(tag => !string.IsNullOrWhiteSpace(tag)).Distinct())
        {
            _tags.Add(new PostTag(Id, tag));
        }

        MarkAsUpdated();
    }

    public void Publish()
    {
        if (Status == PostStatus.Archived)
        {
            throw new InvalidOperationException("Archived posts cannot be published directly.");
        }

        Status = PostStatus.Published;
        PublishedAtUtc = DateTime.UtcNow;
        ScheduledForUtc = null;

        MarkAsUpdated();
    }

    public void Unpublish()
    {
        Status = PostStatus.Draft;
        IsFeatured = false;
        FeaturedAtUtc = null;
        ScheduledForUtc = null;

        MarkAsUpdated();
    }

    public void Archive()
    {
        Status = PostStatus.Archived;
        IsFeatured = false;
        FeaturedAtUtc = null;
        ScheduledForUtc = null;

        MarkAsUpdated();
    }

    public void Schedule(DateTime scheduledForUtc)
    {
        if (scheduledForUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Scheduled date must be in the future.");
        }

        if (Status == PostStatus.Archived)
        {
            throw new InvalidOperationException("Archived posts cannot be scheduled directly.");
        }

        Status = PostStatus.Scheduled;
        ScheduledForUtc = scheduledForUtc;
        IsFeatured = false;
        FeaturedAtUtc = null;

        MarkAsUpdated();
    }

    public void SetFeatured()
    {
        if (Status != PostStatus.Published)
        {
            throw new InvalidOperationException("Only published posts can be featured.");
        }

        IsFeatured = true;
        FeaturedAtUtc = DateTime.UtcNow;

        MarkAsUpdated();
    }

    public void RemoveFeatured()
    {
        IsFeatured = false;
        FeaturedAtUtc = null;

        MarkAsUpdated();
    }
    
    public void AttachAsset(Guid assetId, int displayOrder = 0)
    {
        if (_assets.Any(asset => asset.AssetId == assetId))
        {
            return;
        }

        _assets.Add(new PostAsset(Id, assetId, displayOrder));

        MarkAsUpdated();
    }

    public void DetachAsset(Guid assetId)
    {
        var asset = _assets.FirstOrDefault(asset => asset.AssetId == assetId);

        if (asset is null)
        {
            return;
        }

        _assets.Remove(asset);

        MarkAsUpdated();
    }

    public void SetCoverAsset(Guid? coverAssetId)
    {
        CoverAssetId = coverAssetId;
        MarkAsUpdated();
    }
}

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");

        builder.HasKey(post => post.Id);

        builder.Property(post => post.Id)
            .ValueGeneratedNever();

        builder.Property(post => post.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(post => post.Slug)
            .HasMaxLength(280)
            .IsRequired();

        builder.Property(post => post.Summary)
            .HasMaxLength(1000);

        builder.Property(post => post.Content)
            .IsRequired();

        builder.Property(post => post.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(post => post.IsFeatured)
            .IsRequired();

        builder.Property(post => post.FeaturedAtUtc);

        builder.Property(post => post.PublishedAtUtc);

        builder.Property(post => post.ScheduledForUtc);

        builder.Property(post => post.CoverAssetId);

        builder.Property(post => post.CreatedById)
            .IsRequired();

        builder.Property(post => post.UpdatedById);

        builder.Property(post => post.CreatedAtUtc)
            .IsRequired();

        builder.Property(post => post.UpdatedAtUtc);

        builder.Property(post => post.DeletedAtUtc);

        builder.Property(post => post.IsDeleted)
            .IsRequired();

        builder.HasIndex(post => post.Slug)
            .IsUnique();

        builder.HasIndex(post => post.Status);

        builder.HasIndex(post => post.IsFeatured);

        builder.HasIndex(post => post.PublishedAtUtc);

        builder.HasQueryFilter(post => !post.IsDeleted);
    }
}