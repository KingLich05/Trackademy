namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Топ преподаватель по количеству студентов
/// </summary>
public class TopTeacherDto
{
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Количество студентов у преподавателя
    /// </summary>
    public int StudentCount { get; set; }
    
    /// <summary>
    /// Количество групп у преподавателя
    /// </summary>
    public int GroupCount { get; set; }
    
    /// <summary>
    /// Количество проведенных уроков в этом месяце
    /// </summary>
    public int LessonsThisMonth { get; set; }
    
    /// <summary>
    /// Средняя посещаемость у преподавателя
    /// </summary>
    public decimal AverageAttendanceRate { get; set; }
    
    /// <summary>
    /// Рейтинг преподавателя (условный)
    /// </summary>
    public decimal Rating { get; set; }
}