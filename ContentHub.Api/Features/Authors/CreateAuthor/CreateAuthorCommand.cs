namespace ContentHub.Api.Features.Authors.CreateAuthor;

public sealed class CreateAuthorCommand
{
    public string DisplayName { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Bio { get; set; }

    public Guid? AvatarAssetId { get; set; }

    public bool IsActive { get; set; } = true;
}