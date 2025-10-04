using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Schedule : Entity
{
    /// <summary>
    /// Дни недели как массив 1..7 (1=Пн ... 7=Вс)
    /// </summary>
    public int[]? DaysOfWeek { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
    
    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo  { get; set; }

    public Guid GroupId { get; set; }

    public Guid TeacherId { get; set; }

    public Guid RoomId { get; set; }
    
    public Guid OrganizationId { get; set; }

    #region нав поля

    public Groups Group { get; set; }
    
    public Organization Organization { get; set; }
    
    public User Teacher { get; set; }
    
    public Room Room { get; set; } = null!;

    #endregion
}