using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Room : BaseEntity
{
    public int? Capacity { get; set; }

    #region Навигационные поля

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    #endregion
}