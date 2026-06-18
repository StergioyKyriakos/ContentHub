using System.Security.Cryptography;
using System.Text;

namespace ContentHub.Infrastructure.RateLimiting;

public sealed class RateLimitKeyBuilder
{
    public string BuildLoginKey(
        string emailOrUsername,
        string? ipAddress)
    {
        var normalized = emailOrUsername.Trim().ToUpperInvariant();
        var ip = string.IsNullOrWhiteSpace(ipAddress) ? "unknown" : ipAddress.Trim();
        var raw = $"{normalized}:{ip}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();

        return $"rate-limit:login:{hash}";
    }
}
