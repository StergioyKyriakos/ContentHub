namespace ContentHub.Infrastructure.Authentication;

public sealed class AuthLinkOptions
{
    public const string SectionName = "AuthLinks";

    public string PublicBaseUrl { get; set; } = "http://localhost:5181";

    public string VerifyEmailPath { get; set; } = "/api/auth/verify-email";

    public string ResetPasswordPath { get; set; } = "/api/auth/reset-password";
}
