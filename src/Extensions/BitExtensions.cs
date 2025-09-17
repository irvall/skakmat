namespace skakmat.Extensions;
internal static class BitExtensions
{

    internal static bool Contains(this ulong bitboard, ulong targetBit)
    {
        return (bitboard & targetBit) != 0;
    }

    internal static ulong Exclude(this ulong bitboard, ulong excludedBits)
    {
        return bitboard & ~excludedBits;
    }

    internal static bool ForAll(this ulong bitboard, ulong targetBits)
    {
        return (bitboard & targetBits) == targetBits;
    }

}