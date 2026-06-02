namespace ContentHub.Api.Features.Auth.VerifyEmail;

public sealed class VerifyEmailCommand
{
    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}
