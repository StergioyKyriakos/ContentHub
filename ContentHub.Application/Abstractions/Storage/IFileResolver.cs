namespace ContentHub.Application.Abstractions.Storage;

public interface IFileUrlResolver
{
    string ResolveUrl(string storagePath);
}