namespace skakmat;

internal abstract class Program
{


    [STAThread]
    public static void Main()
    {
        RaylibUtility.IgnoreLogs();
        var engine = new Engine();
        engine.Run();
    }
}