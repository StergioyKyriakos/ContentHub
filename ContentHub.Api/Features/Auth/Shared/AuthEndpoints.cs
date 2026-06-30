namespace ContentHub.Api.Features.Auth.Shared;

public static class AuthEndpoints
{
    public const string Register = "/api/auth/register";

    public const string Login = "/api/auth/login";

    public const string RefreshToken = "/api/auth/refresh-token";

    public const string Logout = "/api/auth/logout";

    public const string GetCurrentUser = "/api/auth/me";

    public const string RequestEmailVerification = "/api/auth/email-verification/request";

    public const string VerifyEmail = "/api/auth/verify-email";

    public const string ForgotPassword = "/api/auth/forgot-password";

    public const string ResetPassword = "/api/auth/reset-password";

    public const string Sessions = "/api/auth/sessions";

    public const string SessionById = "/api/auth/sessions/{id:guid}";

    public const string CurrentSession = "/api/auth/sessions/current";
    
    public const string EnableTwoFactor = "/api/auth/2fa/enable";

    public const string ConfirmTwoFactor = "/api/auth/2fa/confirm";

    public const string DisableTwoFactor = "/api/auth/2fa/disable";

    public const string LoginTwoFactor = "/api/auth/2fa/login";

    public const string OAuthChallenge = "/api/auth/oauth/{provider}/challenge";

    public const string OAuthCallback = "/api/auth/oauth/{provider}/callback";
}
