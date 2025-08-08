namespace Trackademy.Domain.hz;

public class BaseEntity : Entity, IEntityName
{
    public string Name { get; set; }
}