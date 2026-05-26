using ContentHub.Data.Enums;

namespace ContentHub.Api.Features.Notifications.GetNotifications;

public sealed class GetNotificationsQuery
{
    public NotificationStatus? Status { get; set; }

    public NotificationType? Type { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public static ValueTask<GetNotificationsQuery> BindAsync(HttpContext context)
    {
        var query = context.Request.Query;

        return ValueTask.FromResult(new GetNotificationsQuery
        {
            Status = GetEnum<NotificationStatus>(query, "status"),
            Type = GetEnum<NotificationType>(query, "type"),
            Page = GetInt(query, "page") ?? 1,
            PageSize = GetInt(query, "pageSize") ?? 20
        });
    }

    private static string? GetString(IQueryCollection query, string key) =>
        query.TryGetValue(key, out var value) ? value.ToString() : null;

    private static int? GetInt(IQueryCollection query, string key) =>
        int.TryParse(GetString(query, key), out var value) ? value : null;

    private static TEnum? GetEnum<TEnum>(IQueryCollection query, string key)
        where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(GetString(query, key), ignoreCase: true, out var value)
            ? value
            : null;
}
