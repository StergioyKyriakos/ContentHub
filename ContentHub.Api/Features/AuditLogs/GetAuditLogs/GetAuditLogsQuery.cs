using ContentHub.Data.Enums;

namespace ContentHub.Api.Features.AuditLogs.GetAuditLogs;

public sealed class GetAuditLogsQuery
{
    public Guid? ActorUserId { get; set; }

    public AuditAction? Action { get; set; }

    public string? EntityName { get; set; }

    public string? EntityId { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public static ValueTask<GetAuditLogsQuery> BindAsync(HttpContext context)
    {
        var query = context.Request.Query;

        return ValueTask.FromResult(new GetAuditLogsQuery
        {
            ActorUserId = GetGuid(query, "actorUserId"),
            Action = GetEnum<AuditAction>(query, "action"),
            EntityName = GetString(query, "entityName"),
            EntityId = GetString(query, "entityId"),
            From = GetDateTime(query, "from"),
            To = GetDateTime(query, "to"),
            Page = GetInt(query, "page") ?? 1,
            PageSize = GetInt(query, "pageSize") ?? 20
        });
    }

    private static string? GetString(IQueryCollection query, string key) =>
        query.TryGetValue(key, out var value) ? value.ToString() : null;

    private static int? GetInt(IQueryCollection query, string key) =>
        int.TryParse(GetString(query, key), out var value) ? value : null;

    private static Guid? GetGuid(IQueryCollection query, string key) =>
        Guid.TryParse(GetString(query, key), out var value) ? value : null;

    private static DateTime? GetDateTime(IQueryCollection query, string key) =>
        DateTime.TryParse(GetString(query, key), out var value) ? value : null;

    private static TEnum? GetEnum<TEnum>(IQueryCollection query, string key)
        where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(GetString(query, key), ignoreCase: true, out var value)
            ? value
            : null;
}
