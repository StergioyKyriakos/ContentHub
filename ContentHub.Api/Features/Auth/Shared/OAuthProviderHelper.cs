using ContentHub.Api.Extensions;

namespace ContentHub.Api.Features.Auth.Shared;

public static class OAuthProviderHelper
{
    public static string? GetScheme(string provider)
    {
        return provider.Trim().ToLowerInvariant() switch
        {
            "google" => AuthenticationExtensions.GoogleAuthenticationScheme,
            "github" => AuthenticationExtensions.GitHubAuthenticationScheme,
            _ => null
        };
    }

    public static bool IsSupported(string provider)
    {
        return GetScheme(provider) is not null;
    }
}
