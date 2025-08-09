namespace Trackademy.Application.Helper;

public class ExtensionString
{
    public string Str<T>(T value)
    {
        return value?.ToString() ?? string.Empty;
    }
}