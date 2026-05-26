namespace ContentHub.Data.Entities.Common;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    protected Entity(){}

    protected Entity(Guid id)
    {
        Id = id;
    }
}