namespace Trackademy.Domain.Enums;

public enum StatusStudent
{
    /// <summary>
    /// Сдан и проверен
    /// Цвет: зелёный
    /// </summary>
    Checked = 1,

    /// <summary>
    /// Сдан, но на проверке
    /// Цвет: жёлтый
    /// </summary>
    PendingReview = 2,

    /// <summary>
    /// Не сдан, но дедлайн ещё не истёк
    /// Цвет: белый
    /// </summary>
    PendingSubmission = 3,

    /// <summary>
    /// Не сдан, дедлайн истёк
    /// Цвет: красный
    /// </summary>
    Overdue = 4,

    /// <summary>
    /// Сдан, но возвращено на доработку
    /// Цвет: розовый
    /// </summary>
    Returned = 5
}