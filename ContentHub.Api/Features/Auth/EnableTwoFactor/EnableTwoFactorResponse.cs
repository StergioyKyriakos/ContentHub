namespace ContentHub.Api.Features.Auth.EnableTwoFactor;

public sealed class EnableTwoFactorResponse
{
    public string Secret { get; set; } = string.Empty;

    public string AuthenticatorUri { get; set; } = string.Empty;
}