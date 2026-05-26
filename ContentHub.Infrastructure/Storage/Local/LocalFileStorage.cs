using ContentHub.Application.Abstractions.Storage;
using ContentHub.Data.Enums;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.Storage.Local;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly StorageOptions _options;
    private readonly IFileHashCalculator _hashCalculator;

    public LocalFileStorage(
        IOptions<StorageOptions> options,
        IFileHashCalculator hashCalculator)
    {
        _options = options.Value;
        _hashCalculator = hashCalculator;
    }

    public async Task<StoredFile> SaveAsync(
        Stream stream,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var hash = await _hashCalculator.CalculateHashAsync(stream, cancellationToken);

        var extension = Path.GetExtension(originalFileName);
        var safeExtension = string.IsNullOrWhiteSpace(extension)
            ? string.Empty
            : extension.ToLowerInvariant();

        var fileName = $"{Guid.CreateVersion7():N}{safeExtension}";

        var now = DateTime.UtcNow;

        var relativeDirectory = Path.Combine(
            now.Year.ToString("0000"),
            now.Month.ToString("00"),
            now.Day.ToString("00"));

        var absoluteDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            _options.LocalRootPath,
            relativeDirectory);

        Directory.CreateDirectory(absoluteDirectory);

        var absolutePath = Path.Combine(absoluteDirectory, fileName);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        await using var fileStream = File.Create(absolutePath);

        await stream.CopyToAsync(fileStream, cancellationToken);

        var relativeStoragePath = Path.Combine(relativeDirectory, fileName)
            .Replace("\\", "/");

        return new StoredFile
        {
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            Size = stream.CanSeek ? stream.Length : new FileInfo(absolutePath).Length,
            Hash = hash,
            StoragePath = relativeStoragePath,
            Provider = StorageProvider.Local
        };
    }

    public Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var absolutePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            _options.LocalRootPath,
            storagePath);

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var absolutePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            _options.LocalRootPath,
            storagePath);

        return Task.FromResult(File.Exists(absolutePath));
    }
}