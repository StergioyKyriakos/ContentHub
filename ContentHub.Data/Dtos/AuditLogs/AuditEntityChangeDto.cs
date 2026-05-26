namespace ContentHub.Data.Dtos.AuditLogs;

public sealed class AuditEntityChangeDto
{
    public Guid Id { get; set; }

    public string PropertyName { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }
}