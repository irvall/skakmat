using skakmat.Engine;
using skakmat.Helpers;

namespace skakmat;

internal abstract class Program
{

    [STAThread]
    internal static void Main()
    {
        RaylibHelper.IgnoreLogs();
        // TODO: Add commandline options to easy toggle debug/AI
        var engine = new GameEngine();
        engine.Run();
    }
}