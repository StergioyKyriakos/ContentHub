namespace ContentHub.Api.Features.Auth.DisableTwoFactor;

public sealed class DisableTwoFactorResponse
{
    public bool Enabled { get; set; }

    public string Message { get; set; } = "Two-factor authentication disabled.";
}
