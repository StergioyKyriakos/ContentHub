namespace ContentHub.Api.Features.Auth.Shared;

public static class AuthEndpoints
{
    public const string Register = "/api/auth/register";

    public const string Login = "/api/auth/login";

    public const string RefreshToken = "/api/auth/refresh-token";

    public const string Logout = "/api/auth/logout";

    public const string GetCurrentUser = "/api/auth/me";
}