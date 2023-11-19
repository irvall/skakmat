using System.Runtime.InteropServices;
using Raylib_cs;

public class Log
{

    public static void CustomLog(int logType, string text, IntPtr args) { }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TraceLogDelegate(int logType, string text, IntPtr args);

    public static void IgnoreRaylibLogs()
    {
        var traceLog = new Log.TraceLogDelegate(Log.CustomLog);
        unsafe
        {
            Raylib.SetTraceLogCallback((delegate* unmanaged[Cdecl]<int, sbyte*, sbyte*, void>)Marshal.GetFunctionPointerForDelegate(traceLog));
        }
    }



    public static void WriteColor(string text, ConsoleColor consoleColor, bool newLine = true)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = consoleColor;
        if (newLine)
        {
            Console.WriteLine(text);
        }
        else
        {
            Console.Write(text);
        }
        Console.ForegroundColor = previousColor;
    }

    public static string BoldText(string text)
    {
        return $"\u001b[1m{text}\u001b[0m";
    }
}