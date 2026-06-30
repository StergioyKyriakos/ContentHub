namespace ContentHub.Api.Features.Auth.ConfirmTwoFactor;

public sealed class ConfirmTwoFactorCommand
{
    public string Code { get; set; } = string.Empty;
}