using ContentHub.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.OAuthCallback;

public static class OAuthUsernameBuilder
{
    public static async Task<string> CreateUniqueUsernameAsync(
        ContentHubDbContext db,
        string email,
        string provider,
        string providerUserId,
        CancellationToken ct)
    {
        var baseUsername = BuildUsername(email, provider, providerUserId);
        var username = baseUsername;
        var counter = 1;

        while (await db.Users.AnyAsync(
                   user => user.NormalizedUsername == username.ToUpperInvariant(),
                   ct))
        {
            var suffix = counter.ToString();
            username = $"{TrimToMax(baseUsername, 100 - suffix.Length)}{suffix}";
            counter++;
        }

        return username;
    }

    public static string TrimToMax(string value, int maxLength)
    {
        var trimmed = value.Trim();

        return trimmed.Length <= maxLength
            ? trimmed
            : trimmed[..maxLength];
    }

    private static string BuildUsername(
        string email,
        string provider,
        string providerUserId)
    {
        var source = email.Contains('@')
            ? email[..email.IndexOf('@')]
            : $"{provider}_{providerUserId}";

        var characters = source
            .Where(character => char.IsLetterOrDigit(character) || character is '_' or '-' or '.')
            .ToArray();

        var username = new string(characters).Trim('.', '-', '_');

        if (username.Length < 3)
        {
            username = $"{provider}_{providerUserId}";
        }

        return TrimToMax(username, 100);
    }
}
