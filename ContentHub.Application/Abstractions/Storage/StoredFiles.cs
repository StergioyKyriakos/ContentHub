using ContentHub.Data.Enums;

namespace ContentHub.Application.Abstractions.Storage;

public sealed class StoredFile
{
    public string FileName { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public string Hash { get; set; } = null!;

    public string StoragePath { get; set; } = null!;

    public StorageProvider Provider { get; set; }
}