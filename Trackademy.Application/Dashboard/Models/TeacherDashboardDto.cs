namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Дашборд для преподавателя
/// </summary>
public class TeacherDashboardDto
{
    /// <summary>
    /// Общее количество групп преподавателя
    /// </summary>
    public int TotalGroups { get; set; }
    
    /// <summary>
    /// Количество непроверенных работ (submissions в статусе Submitted)
    /// </summary>
    public int UngradedSubmissions { get; set; }
    
    /// <summary>
    /// Количество занятий на сегодня
    /// </summary>
    public int LessonsToday { get; set; }
    
    /// <summary>
    /// Расписание занятий на сегодня
    /// </summary>
    public List<TeacherTodayScheduleDto> TodaySchedule { get; set; } = new();
}

/// <summary>
/// Информация о занятии на сегодня для преподавателя
/// </summary>
public class TeacherTodayScheduleDto
{
    /// <summary>
    /// ID урока
    /// </summary>
    public Guid LessonId { get; set; }
    
    /// <summary>
    /// Время начала
    /// </summary>
    public TimeOnly StartTime { get; set; }
    
    /// <summary>
    /// Время окончания
    /// </summary>
    public TimeOnly EndTime { get; set; }
    
    /// <summary>
    /// Название группы
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    
    /// <summary>
    /// Название предмета
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Номер/название комнаты
    /// </summary>
    public string? RoomName { get; set; }
    
    /// <summary>
    /// Статус урока (прошел или предстоящий)
    /// </summary>
    public bool IsPast { get; set; }
    
    /// <summary>
    /// Статус посещаемости (если урок прошел)
    /// Процент присутствующих студентов
    /// </summary>
    public decimal? AttendanceRate { get; set; }
    
    /// <summary>
    /// Количество присутствующих студентов (если урок прошел)
    /// </summary>
    public int? PresentCount { get; set; }
    
    /// <summary>
    /// Общее количество студентов в группе
    /// </summary>
    public int? TotalStudents { get; set; }
}
