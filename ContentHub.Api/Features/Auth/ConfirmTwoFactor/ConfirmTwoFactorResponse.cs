namespace ContentHub.Api.Features.Auth.ConfirmTwoFactor;

public sealed class ConfirmTwoFactorResponse
{
    public bool Enabled { get; set; }

    public string Message { get; set; } = "Two-factor authentication enabled.";
}