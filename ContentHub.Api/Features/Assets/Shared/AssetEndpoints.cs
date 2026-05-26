namespace ContentHub.Api.Features.Assets.Shared;

public static class AssetEndpoints
{
    public const string Upload = "/api/assets/upload";

    public const string GetById = "/api/assets/{id:guid}";

    public const string GetAll = "/api/assets";

    public const string Delete = "/api/assets/{id:guid}";

    public const string AttachToPost = "/api/posts/{postId:guid}/assets/{assetId:guid}";

    public const string DetachFromPost = "/api/posts/{postId:guid}/assets/{assetId:guid}";
}