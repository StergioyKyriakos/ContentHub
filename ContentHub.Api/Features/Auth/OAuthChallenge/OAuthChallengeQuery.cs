using Microsoft.AspNetCore.Mvc;

namespace ContentHub.Api.Features.Auth.OAuthChallenge;

public sealed class OAuthChallengeQuery
{
    [FromRoute(Name = "provider")]
    public string Provider { get; set; } = string.Empty;
}
