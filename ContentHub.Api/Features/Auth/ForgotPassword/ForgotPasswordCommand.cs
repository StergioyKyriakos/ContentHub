namespace ContentHub.Api.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommand
{
    public string Email { get; set; } = string.Empty;
}
