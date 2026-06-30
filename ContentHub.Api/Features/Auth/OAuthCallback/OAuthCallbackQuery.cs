using Microsoft.AspNetCore.Mvc;

namespace ContentHub.Api.Features.Auth.OAuthCallback;

public sealed class OAuthCallbackQuery
{
    [FromRoute(Name = "provider")]
    public string Provider { get; set; } = string.Empty;
}
