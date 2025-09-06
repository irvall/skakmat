namespace skakmat.Utilities;
internal static class NumberExtensions
{

    public static bool Contains(this ulong bitboard, ulong targetBit)
    {
        return (bitboard & targetBit) != 0;
    }

    public static ulong Exclude(this ulong bitboard, ulong excludedBits)
    {
        return bitboard & ~excludedBits;
    }

}