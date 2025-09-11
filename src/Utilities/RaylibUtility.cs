using System.Runtime.InteropServices;
using Raylib_cs;

namespace skakmat.Utilities;

internal abstract class RaylibUtility
{
    private static void CustomLog(int logType, string text, nint args)
    {
    }

    internal static void IgnoreLogs()
    {
        var traceLog = new TraceLogDelegate(CustomLog);
        unsafe
        {
            Raylib.SetTraceLogCallback(
                (delegate* unmanaged[Cdecl]<int, sbyte*, sbyte*, void>)Marshal.GetFunctionPointerForDelegate(traceLog));
        }
    }

    internal static void WriteColor(string text, ConsoleColor consoleColor, bool newLine = true)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = consoleColor;
        if (newLine)
            Console.WriteLine(text);
        else
            Console.Write(text);
        Console.ForegroundColor = previousColor;
    }

    internal static string BoldText(string text)
    {
        return $"\u001b[1m{text}\u001b[0m";
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void TraceLogDelegate(int logType, string text, nint args);

    internal static int GetWindowHeightDynamically()
    {
        Raylib.InitWindow(0, 0, "Temporary 0x0 window to get screen size");
        var windowHeight = (int)(Raylib.GetScreenHeight() / 1.5);
        Raylib.CloseWindow();
        return windowHeight;
    }

    private static string SafeGetPath(string folder, string fileName)
    {
        var execDir = AppContext.BaseDirectory;
        var path = Path.Combine(execDir, "assets", folder, fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Asset not found: {path}");
        return path;
    }

    internal static Texture2D LoadSpritesheet(string fileName)
    {
        var path = SafeGetPath("spritesheets", fileName);
        return Raylib.LoadTexture(path);
    }

    internal static Sound LoadSound(string fileName)
    {
        var path = SafeGetPath("sounds", fileName);
        return Raylib.LoadSound(path);
    }
}