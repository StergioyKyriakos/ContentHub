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

    public static ApiError LoginRateLimited =>
        ApiError.Create(
            code: "auth.login_rate_limited",
            message: "Too many failed login attempts. Please try again later.");

    public static ApiError EmailDeliveryFailed =>
        ApiError.Create(
            code: "auth.email_delivery_failed",
            message: "The account was saved, but the email could not be sent. Please request a new email later.");

    public static ApiError InvalidRefreshToken =>
        ApiError.Create(
            code: "auth.invalid_refresh_token",
            message: "Invalid refresh token.");

    public static ApiError InvalidEmailVerificationToken =>
        ApiError.Create(
            code: "auth.invalid_email_verification_token",
            message: "Invalid or expired email verification token.");

    public static ApiError InvalidPasswordResetToken =>
        ApiError.Create(
            code: "auth.invalid_password_reset_token",
            message: "Invalid or expired password reset token.");

    public static ApiError UserNotFound =>
        ApiError.Create(
            code: "auth.user_not_found",
            message: "User was not found.");

    public static ApiError UserDisabled =>
        ApiError.Create(
            code: "auth.user_disabled",
            message: "This user account is disabled.");

    public static ApiError SessionNotFound =>
        ApiError.Create(
            code: "auth.session_not_found",
            message: "Session was not found.");

    public static ApiError ExternalLoginFailed =>
        ApiError.Create(
            code: "auth.external_login_failed",
            message: "External login failed.");

    public static ApiError ExternalEmailMissing =>
        ApiError.Create(
            code: "auth.external_email_missing",
            message: "The external provider did not return a verified email address.");
}
