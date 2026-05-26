using System.Text.Json;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Entities.AuditLogs;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;

namespace ContentHub.Api.Common.Auditing;

public sealed class AuditLogWriter
{
    private readonly ContentHubDbContext _db;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogWriter(
        ContentHubDbContext db,
        ICurrentUserProvider currentUserProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _currentUserProvider = currentUserProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public void Add(
        AuditAction action,
        string entityName,
        string? entityId,
        object? oldValues = null,
        object? newValues = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var auditLog = new AuditLog(
            actorUserId: _currentUserProvider.UserId,
            action: action,
            entityName: entityName,
            entityId: entityId,
            oldValuesJson: Serialize(oldValues),
            newValuesJson: Serialize(newValues),
            ipAddress: httpContext?.Connection.RemoteIpAddress?.ToString(),
            userAgent: httpContext?.Request.Headers.UserAgent.ToString());

        _db.AuditLogs.Add(auditLog);
    }

    public void AddAnonymous(
        AuditAction action,
        string entityName,
        string? entityId,
        object? oldValues = null,
        object? newValues = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var auditLog = new AuditLog(
            actorUserId: null,
            action: action,
            entityName: entityName,
            entityId: entityId,
            oldValuesJson: Serialize(oldValues),
            newValuesJson: Serialize(newValues),
            ipAddress: httpContext?.Connection.RemoteIpAddress?.ToString(),
            userAgent: httpContext?.Request.Headers.UserAgent.ToString());

        _db.AuditLogs.Add(auditLog);
    }

    private static string? Serialize(object? value)
    {
        if (value is null)
        {
            return null;
        }

        return JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}