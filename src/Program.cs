// See https://aka.ms/new-console-template for more information

namespace skakmat;

internal abstract class Program
{
    [STAThread]
    public static void Main()
    {
        var board = new Board();
    }
}