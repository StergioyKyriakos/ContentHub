namespace ContentHub.Application.Abstractions.Storage;

public interface IFileStorage
{
    Task<StoredFile> SaveAsync(
        Stream stream,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}