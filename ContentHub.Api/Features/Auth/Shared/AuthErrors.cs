using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.Auth.Shared;

public static class AuthErrors
{
    public static ApiError EmailAlreadyExists =>
        ApiError.Create(
            code: "auth.email_already_exists",
            message: "A user with this email already exists.");

    public static ApiError UsernameAlreadyExists =>
        ApiError.Create(
            code: "auth.username_already_exists",
            message: "A user with this username already exists.");

    public static ApiError InvalidCredentials =>
        ApiError.Create(
            code: "auth.invalid_credentials",
            message: "Invalid email, username, or password.");

    public static ApiError InvalidRefreshToken =>
        ApiError.Create(
            code: "auth.invalid_refresh_token",
            message: "Invalid refresh token.");

    public static ApiError UserNotFound =>
        ApiError.Create(
            code: "auth.user_not_found",
            message: "User was not found.");

    public static ApiError UserDisabled =>
        ApiError.Create(
            code: "auth.user_disabled",
            message: "This user account is disabled.");
}