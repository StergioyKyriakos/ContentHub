namespace ContentHub.Api.Features.Auth.OAuthChallenge;

public sealed class OAuthChallengeResponse
{
    public required string Provider { get; set; }

    public required string CallbackPath { get; set; }
}
