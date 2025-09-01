using skakmat;

public static class SquareExtensions
{

    public static ulong Absolute(this BoardSquare bs)
    {
        var squareAsInt = (int)bs;
        return 1UL << squareAsInt;
    }
}