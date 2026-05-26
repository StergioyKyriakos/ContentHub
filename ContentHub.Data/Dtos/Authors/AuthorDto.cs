namespace ContentHub.Data.Dtos.Authors;

public sealed class AuthorDto
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Bio { get; set; }

    public Guid? AvatarAssetId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}