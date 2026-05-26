using ContentHub.Data.Enums;

namespace ContentHub.Api.Features.Assets.Shared;

public static class AssetTypeResolver
{
    public static AssetType Resolve(string contentType)
    {
        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return AssetType.Image;
        }

        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            return AssetType.Video;
        }

        if (contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
        {
            return AssetType.Audio;
        }

        if (contentType is "application/pdf")
        {
            return AssetType.Document;
        }

        return AssetType.Other;
    }
}