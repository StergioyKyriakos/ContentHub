using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.Notifications.Shared;

public static class NotificationErrors
{
    public static ApiError NotFound =>
        ApiError.Create(
            code: "notifications.not_found",
            message: "Notification was not found.");

    public static ApiError UserNotAuthenticated =>
        ApiError.Create(
            code: "notifications.user_not_authenticated",
            message: "User is not authenticated.");

    public static ApiError PreferenceNotFound =>
        ApiError.Create(
            code: "notifications.preference_not_found",
            message: "Notification preference was not found.");
}