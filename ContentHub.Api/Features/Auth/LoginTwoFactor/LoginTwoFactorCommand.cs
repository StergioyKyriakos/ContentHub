namespace ContentHub.Api.Features.Auth.LoginTwoFactor;

public sealed class LoginTwoFactorCommand
{
    public Guid UserId { get; set; }

    public string Code { get; set; } = string.Empty;
}