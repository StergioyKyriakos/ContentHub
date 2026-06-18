using ContentHub.Application.Abstractions.Storage;
using ContentHub.Data.Enums;
using ContentHub.Infrastructure.Storage.Cloud;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.Storage;

public sealed class FileUrlResolver : IFileUrlResolver
{
    private readonly StorageOptions _options;
    private readonly AzureBlobStorageOptions _azureBlobOptions;
    private readonly S3StorageOptions _s3Options;

    public FileUrlResolver(
        IOptions<StorageOptions> options,
        IOptions<AzureBlobStorageOptions> azureBlobOptions,
        IOptions<S3StorageOptions> s3Options)
    {
        _options = options.Value;
        _azureBlobOptions = azureBlobOptions.Value;
        _s3Options = s3Options.Value;
    }

    public string ResolveUrl(
        string storagePath,
        StorageProvider? provider = null)
    {
        var baseUrl = GetBaseUrl(provider).TrimEnd('/');
        var path = storagePath.Replace("\\", "/").TrimStart('/');

        return $"{baseUrl}/{path}";
    }

    private string GetBaseUrl(StorageProvider? provider)
    {
        return provider switch
        {
            StorageProvider.AzureBlob when !string.IsNullOrWhiteSpace(_azureBlobOptions.PublicBaseUrl) =>
                _azureBlobOptions.PublicBaseUrl,
            StorageProvider.S3 when !string.IsNullOrWhiteSpace(_s3Options.PublicBaseUrl) =>
                _s3Options.PublicBaseUrl,
            _ => _options.PublicBaseUrl
        };
    }
}
