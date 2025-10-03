using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Room : BaseEntity
{
    public required int Capacity { get; set; }
    
    public Guid OrganizationId { get; set; }

    #region Навигационные поля
    
    public Organization Organization { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    #endregion
}