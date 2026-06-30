using ContentHub.Api.Features.Auth.Login;

namespace ContentHub.Api.Features.Auth.OAuthCallback;

public sealed class OAuthCallbackResponse
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public required string Provider { get; set; }

    public bool CreatedUser { get; set; }

    public bool LinkedExternalAccount { get; set; }

    public required AuthUserResponse User { get; set; }
}
