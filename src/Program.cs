using skakmat.Engine;
using skakmat.Rendering;
using skakmat.Utilites;

namespace skakmat;

internal abstract class Program
{

    [STAThread]
    public static void Main()
    {
        RaylibUtility.IgnoreLogs();
        var engine = new GameEngine();
        engine.Run();
        // var visualiser = new BitboardVisualiser();
        // visualiser.Run();
    }
}