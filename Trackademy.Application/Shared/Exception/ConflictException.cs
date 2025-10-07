namespace Trackademy.Application.Shared.Exception;

public class ConflictException: System.Exception
{
    /// <summary>
    /// Конфликт — например, когда сущность уже существует или есть пересечение по времени.
    /// </summary>
    public ConflictException(string message) : base(message) { }
    
    /// <summary>
    /// Сущность не найдена (например, когда ищем по Id и ничего нет).
    /// </summary>
    public class NotFoundException : System.Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}