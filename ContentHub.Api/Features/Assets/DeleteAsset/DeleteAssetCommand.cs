namespace ContentHub.Api.Features.Assets.DeleteAsset;

public sealed class DeleteAssetCommand
{
    public Guid Id { get; set; }
    public bool Force { get; set; }
}