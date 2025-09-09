using skakmat.Engine;
using skakmat.Utilities;

namespace skakmat;

internal abstract class Program
{

    [STAThread]
    public static void Main()
    {
        RaylibUtility.IgnoreLogs();
        // TODO: Add commandline options to easy toggle debug/AI
        var engine = new GameEngine(true);
        engine.Run();
    }
}