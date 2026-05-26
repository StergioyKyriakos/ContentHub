namespace ContentHub.Api.Features.Authors.UpdateAuthor;

public sealed class UpdateAuthorCommand
{
    public string DisplayName { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Bio { get; set; }

    public Guid? AvatarAssetId { get; set; }

    public bool IsActive { get; set; }
}