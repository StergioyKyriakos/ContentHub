namespace ContentHub.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local";

    public string RootPath { get; set; } = "storage/assets";

    public string LocalRootPath { get; set; } = "storage/assets";

    public string PublicBaseUrl { get; set; } = "/assets";

    public string EffectiveLocalRootPath =>
        !string.IsNullOrWhiteSpace(LocalRootPath)
            ? LocalRootPath
            : RootPath;

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    public string[] AllowedContentTypes { get; set; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "application/pdf"
    ];
}
