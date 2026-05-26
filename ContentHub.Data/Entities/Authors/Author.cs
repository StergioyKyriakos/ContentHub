using ContentHub.Data.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Authors;

public sealed class Author : AggregateRoot
{
    public Author()
    {
    }

    public Author(
        string displayName,
        string slug,
        string? bio,
        Guid? avatarAssetId,
        bool isActive,
        Guid? userId = null)
    {
        DisplayName = displayName.Trim();
        Slug = slug.Trim().ToLowerInvariant();

        Bio = string.IsNullOrWhiteSpace(bio)
            ? null
            : bio.Trim();

        AvatarAssetId = avatarAssetId;
        IsActive = isActive;
        UserId = userId;
    }

    public string DisplayName { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Bio { get; set; }

    public Guid? AvatarAssetId { get; set; }

    public bool IsActive { get; set; }
    
    public Guid? UserId { get; set; }

    public void Update(
        string displayName,
        string slug,
        string? bio,
        Guid? avatarAssetId,
        bool isActive,
        Guid? userId = null)
    {
        DisplayName = displayName.Trim();
        Slug = slug.Trim().ToLowerInvariant();

        Bio = string.IsNullOrWhiteSpace(bio)
            ? null
            : bio.Trim();

        AvatarAssetId = avatarAssetId;
        IsActive = isActive;
        UserId = userId;

        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }
}

public sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("authors");

        builder.HasKey(author => author.Id);

        builder.Property(author => author.Id)
            .ValueGeneratedNever();

        builder.Property(author => author.DisplayName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(author => author.Slug)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(author => author.Bio)
            .HasMaxLength(3000);

        builder.Property(author => author.AvatarAssetId);

        builder.Property(author => author.IsActive)
            .IsRequired();

        builder.Property(author => author.CreatedAtUtc)
            .IsRequired();

        builder.Property(author => author.UpdatedAtUtc);

        builder.Property(author => author.DeletedAtUtc);

        builder.Property(author => author.IsDeleted)
            .IsRequired();

        builder.HasIndex(author => author.Slug)
            .IsUnique();
        
        builder.Property(author => author.UserId);

        builder.HasIndex(author => author.UserId);

        builder.HasIndex(author => author.IsActive);

        builder.HasQueryFilter(author => !author.IsDeleted);
    }
}