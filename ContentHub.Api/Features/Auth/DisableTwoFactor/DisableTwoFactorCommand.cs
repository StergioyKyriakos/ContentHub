namespace ContentHub.Api.Features.Auth.DisableTwoFactor;

public sealed class DisableTwoFactorCommand
{
    public string Code { get; set; } = string.Empty;
}