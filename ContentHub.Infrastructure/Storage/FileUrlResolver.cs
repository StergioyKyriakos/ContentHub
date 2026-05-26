using ContentHub.Application.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.Storage;

public sealed class FileUrlResolver : IFileUrlResolver
{
    private readonly StorageOptions _options;

    public FileUrlResolver(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public string ResolveUrl(string storagePath)
    {
        var baseUrl = _options.PublicBaseUrl.TrimEnd('/');
        var path = storagePath.Replace("\\", "/").TrimStart('/');

        return $"{baseUrl}/{path}";
    }
}