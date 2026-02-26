using Microsoft.Maui.Handlers;

namespace LearnWithCircle.Controls;

public partial class RotatableImageHandler : ImageHandler
{
#if DEBUG
    protected static void Log(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[RotatableImage] {message}");
        Console.WriteLine($"[RotatableImage] {message}");
        LogPlatform(message);
    }

    static partial void LogPlatform(string message);
#else
    protected static void Log(string message)
    {
    }
#endif
}
