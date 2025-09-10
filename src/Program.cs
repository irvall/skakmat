using skakmat.Engine;
using skakmat.Utilities;

namespace skakmat;

internal abstract class Program
{

    [STAThread]
    internal static void Main()
    {
        RaylibUtility.IgnoreLogs();
        // TODO: Add commandline options to easy toggle debug/AI
        var engine = new GameEngine();
        engine.Run();
    }
}