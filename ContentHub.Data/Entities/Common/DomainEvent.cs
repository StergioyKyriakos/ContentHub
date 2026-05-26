namespace ContentHub.Data.Entities.Common;

public abstract record DomainEvent
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}