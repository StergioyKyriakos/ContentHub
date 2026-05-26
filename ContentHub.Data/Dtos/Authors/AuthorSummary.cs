namespace ContentHub.Data.Dtos.Authors;

public sealed class AuthorSummaryDto
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public Guid? AvatarAssetId { get; set; }

    public bool IsActive { get; set; }
}