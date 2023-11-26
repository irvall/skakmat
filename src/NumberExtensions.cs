namespace skakmat;
internal static class NumberExtensions
{

    public static bool Contains(this ulong bitboard, ulong targetBit)
    {
        return (bitboard & targetBit) != 0;
    }

}