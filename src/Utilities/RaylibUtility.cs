using System.Runtime.InteropServices;
using Raylib_cs;

namespace skakmat.Utilites;

public abstract class RaylibUtility
{
    private static void CustomLog(int logType, string text, nint args)
    {
    }

    public static void IgnoreLogs()
    {
        var traceLog = new TraceLogDelegate(CustomLog);
        unsafe
        {
            Raylib.SetTraceLogCallback(
                (delegate* unmanaged[Cdecl]<int, sbyte*, sbyte*, void>)Marshal.GetFunctionPointerForDelegate(traceLog));
        }
    }

    public static void WriteColor(string text, ConsoleColor consoleColor, bool newLine = true)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = consoleColor;
        if (newLine)
            Console.WriteLine(text);
        else
            Console.Write(text);
        Console.ForegroundColor = previousColor;
    }

    public static string BoldText(string text)
    {
        return $"\u001b[1m{text}\u001b[0m";
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void TraceLogDelegate(int logType, string text, nint args);

    public static int GetWindowHeightDynamically()
    {
        Raylib.InitWindow(0, 0, "Temporary 0x0 window to get screen size");
        var windowHeight = (int)(Raylib.GetScreenHeight() / 1.5);
        Raylib.CloseWindow();
        return windowHeight;
    }

    public static Texture2D LoadTextureChecked(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Asset not found: {path}");
        return Raylib.LoadTexture(path);
    }
}