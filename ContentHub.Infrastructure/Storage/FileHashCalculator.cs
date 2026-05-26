using System.Security.Cryptography;
using ContentHub.Application.Abstractions.Storage;

namespace ContentHub.Infrastructure.Storage;

public sealed class FileHashCalculator : IFileHashCalculator
{
    public async Task<string> CalculateHashAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var sha256 = SHA256.Create();

        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}