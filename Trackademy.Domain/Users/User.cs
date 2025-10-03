using Trackademy.Domain.Enums;
using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class User : Entity
{
    public required string Login { get; set; }

    public string FullName { get; set; }
    
    public string Email { get; set; }
    
    public string PasswordHash { get; set; }
    
    public string? PhotoPath { get; set; }
    
    public string Phone { get; set; }

    public string? ParentPhone { get; set; }

    public DateTime CreatedDate { get; set; }

    public RoleEnum Role { get; set; } = RoleEnum.Student;
    
    public Guid OrganizationId { get; set; }
    
    public List<Guid> GroupIds { get; set; } = new List<Guid>();
    
    #region нав свойства

    public ICollection<Groups> Groups { get; set; } = new List<Groups>();

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    
    public Organization Organization { get; set; }

    #endregion
}