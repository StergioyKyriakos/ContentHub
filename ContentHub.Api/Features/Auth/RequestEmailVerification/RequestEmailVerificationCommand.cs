namespace ContentHub.Api.Features.Auth.RequestEmailVerification;

public sealed class RequestEmailVerificationCommand
{
    public string Email { get; set; } = string.Empty;
}
