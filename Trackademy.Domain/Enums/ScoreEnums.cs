namespace Trackademy.Domain.Enums
{
    /// <summary>
    /// Статус оценки
    /// </summary>
    public enum ScoreStatus
    {
        /// <summary>
        /// Черновик оценки
        /// </summary>
        Draft = 0,
        
        /// <summary>
        /// Окончательная оценка
        /// </summary>
        Final = 1,
        
        /// <summary>
        /// Оценка на пересдаче
        /// </summary>
        Retake = 2,
        
        /// <summary>
        /// Отмененная оценка
        /// </summary>
        Cancelled = 3
    }
}