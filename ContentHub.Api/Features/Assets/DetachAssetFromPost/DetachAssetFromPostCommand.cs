namespace ContentHub.Api.Features.Assets.DetachAssetFromPost;

public sealed class DetachAssetFromPostCommand
{
    public Guid PostId { get; set; }
    public Guid AssetId { get; set; }
}