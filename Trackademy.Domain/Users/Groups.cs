using Trackademy.Domain.Common;
using Trackademy.Domain.Enums;

namespace Trackademy.Domain.Users;

public class Groups : Entity
{
    public string Name { get; set; }
    public string Code { get; set; }
    
    /// <summary>
    /// Описание группы если необоходимо это или все таки уровень это больше не описание а доп информация для группы
    /// </summary>
    public string? Level { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid OrganizationId { get; set; }
    
    public Guid SubjectId {get; set;}
    
    /// <summary>
    /// Тип оплаты для группы: Monthly (ежемесячно) или OneTime (разово за весь курс)
    /// </summary>
    public PaymentType PaymentType { get; set; } = PaymentType.Monthly;
    
    /// <summary>
    /// Стоимость обучения в месяц (для Monthly) или за весь курс (для OneTime)
    /// </summary>
    public decimal MonthlyPrice { get; set; }
    
    /// <summary>
    /// Дата окончания курса (используется для OneTime платежей)
    /// </summary>
    public DateTime? CourseEndDate { get; set; }

    #region Navigation properties

    public ICollection<User> Students { get; set; } = new List<User>();
    public ICollection<GroupStudent> GroupStudents { get; set; } = new List<GroupStudent>();
    public Subject Subject { get; set; }
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Organization Organization { get; set; }

    #endregion
}