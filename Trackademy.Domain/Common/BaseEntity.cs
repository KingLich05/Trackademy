namespace Trackademy.Domain.Common;

public class BaseEntity : Entity, IEntityName
{
    public string Name { get; set; }
}