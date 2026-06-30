using ContentHub.Api.Features.Auth.Login;

namespace ContentHub.Api.Features.Auth.LoginTwoFactor;

public sealed class LoginTwoFactorResponse
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public required AuthUserResponse User { get; set; }
}
