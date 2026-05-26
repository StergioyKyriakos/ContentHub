namespace ContentHub.Api.Features.Assets.AttachAssetToPost;

public sealed class AttachAssetToPostCommand
{
    public Guid PostId { get; set; }
    public Guid AssetId { get; set; }
}