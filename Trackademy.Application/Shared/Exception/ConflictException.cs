namespace Trackademy.Application.Shared.Exception;

public class ConflictException: System.Exception
{
    public ConflictException(string message) : base(message) { }
}