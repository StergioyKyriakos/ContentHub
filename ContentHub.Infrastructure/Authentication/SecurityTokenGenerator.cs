using System.Security.Cryptography;
using System.Text;
using ContentHub.Application.Abstractions.Authentication;

namespace ContentHub.Infrastructure.Authentication;

public sealed class SecurityTokenGenerator : ISecurityTokenGenerator
{
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string Hash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }
}
