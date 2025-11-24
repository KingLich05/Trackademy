namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Дашборд для студента
/// </summary>
public class StudentDashboardDto
{
    /// <summary>
    /// Средний балл по домашним заданиям
    /// </summary>
    public decimal? AverageGrade { get; set; }
    
    /// <summary>
    /// Процент посещаемости
    /// </summary>
    public decimal AttendanceRate { get; set; }
    
    /// <summary>
    /// Количество активных заданий (не сданные + просроченные + возвращенные)
    /// </summary>
    public int ActiveAssignments { get; set; }
    
    /// <summary>
    /// Список активных заданий
    /// </summary>
    public List<StudentActiveAssignmentDto> ActiveAssignmentsList { get; set; } = new();
    
    /// <summary>
    /// Расписание на сегодня
    /// </summary>
    public List<StudentTodayScheduleDto> TodaySchedule { get; set; } = new();
    
    /// <summary>
    /// Последние 5 оценок
    /// </summary>
    public List<StudentRecentGradeDto> RecentGrades { get; set; } = new();
}

/// <summary>
/// Активное задание студента
/// </summary>
public class StudentActiveAssignmentDto
{
    /// <summary>
    /// ID задания
    /// </summary>
    public Guid AssignmentId { get; set; }
    
    /// <summary>
    /// Описание задания
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Название предмета
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Название группы
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    
    /// <summary>
    /// Дедлайн
    /// </summary>
    public DateTime DueDate { get; set; }
    
    /// <summary>
    /// Статус задания
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Просрочено ли задание
    /// </summary>
    public bool IsOverdue { get; set; }
}

/// <summary>
/// Занятие на сегодня для студента
/// </summary>
public class StudentTodayScheduleDto
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
    /// Название предмета
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Название группы
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    
    /// <summary>
    /// Номер/название комнаты
    /// </summary>
    public string? RoomName { get; set; }
    
    /// <summary>
    /// Имя преподавателя
    /// </summary>
    public string TeacherName { get; set; } = string.Empty;
}

/// <summary>
/// Последняя оценка студента
/// </summary>
public class StudentRecentGradeDto
{
    /// <summary>
    /// Название предмета
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Оценка
    /// </summary>
    public decimal Grade { get; set; }
    
    /// <summary>
    /// Дата выставления оценки
    /// </summary>
    public DateTime GradedAt { get; set; }
}
