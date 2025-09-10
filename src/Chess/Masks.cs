namespace skakmat.Chess;

internal class Masks
{
    internal static ulong FileA { get; } = 0x0101010101010101;
    internal static ulong FileB { get; } = 0x0202020202020202;
    internal static ulong FileC { get; } = 0x0404040404040404;
    internal static ulong FileD { get; } = 0x0808080808080808;
    internal static ulong FileE { get; } = 0x1010101010101010;
    internal static ulong FileF { get; } = 0x2020202020202020;
    internal static ulong FileG { get; } = 0x4040404040404040;
    internal static ulong FileH { get; } = 0x8080808080808080;
    internal static ulong Rank1 { get; } = 0xFF00000000000000;
    internal static ulong Rank2 { get; } = 0x00FF000000000000;
    internal static ulong Rank3 { get; } = 0x0000FF0000000000;
    internal static ulong Rank4 { get; } = 0x000000FF00000000;
    internal static ulong Rank5 { get; } = 0x00000000FF000000;
    internal static ulong Rank6 { get; } = 0x0000000000FF0000;
    internal static ulong Rank7 { get; } = 0x000000000000FF00;
    internal static ulong Rank8 { get; } = 0x00000000000000FF;
    internal static ulong Center { get; } = 0x0000001818000000;
    internal static ulong Corners { get; } = 0x8100000000000081;
    internal static ulong CornersAndCenter { get; } = 0x8100001818000081;
    internal static ulong CornersAndCenterAndAdjacent { get; } = 0xFF000018181800FF;
    internal static ulong Edge = Rank1 | Rank8 | FileA | FileH;

    internal static ulong WhiteKingShortGap = 0x6000000000000000;
    internal static ulong WhiteKingLongGap = 0xe00000000000000;
    private const ulong WhiteKingTryCastleShort = 0xc000000000000000;
    private static ulong WhiteKingTryCastleLong = 0x700000000000000;


    internal const ulong BlackKingShortGap = 0x60;
    internal static ulong BlackKingLongGap = 0xe;
    private static ulong BlackKingTryCastleShort = 0xc0;
    internal static ulong BlackKingTryCastleLong = 0x7;

    internal static ulong QueenSideCastlePath(bool isWhite) =>
        isWhite ? WhiteKingLongGap : BlackKingLongGap;

    internal static ulong KingSideCastlePath(bool isWhite) =>
        isWhite ? WhiteKingShortGap : BlackKingShortGap;

    internal static ulong KingAttemptsShortCastle(bool isWhite) =>
        isWhite ? WhiteKingTryCastleShort : BlackKingTryCastleShort;

    internal static ulong KingAttemptsLongCastle(bool isWhite) =>
        isWhite ? WhiteKingTryCastleLong : BlackKingTryCastleLong;

    internal static ulong KingStartSquare(bool isWhite) =>
        isWhite ? BoardSquares.Squares.E1.AsBit() : BoardSquares.Squares.E8.AsBit();

    internal static ulong RookRightCorner(bool isWhite) =>
        isWhite ? BoardSquares.Squares.H1.AsBit() : BoardSquares.Squares.H8.AsBit();

    internal static ulong RookLeftCorner(bool isWhite) =>
        isWhite ? BoardSquares.Squares.A1.AsBit() : BoardSquares.Squares.A8.AsBit();

    internal static ulong RookShortCastlePosition(bool isWhite) =>
        isWhite ? BoardSquares.Squares.F1.AsBit() : BoardSquares.Squares.F8.AsBit();

    internal static ulong RookLongCastlePosition(bool isWhite) =>
        isWhite ? BoardSquares.Squares.D1.AsBit() : BoardSquares.Squares.D8.AsBit();

    internal static ulong KingShortCastlePosition(bool isWhite) =>
        isWhite ? BoardSquares.Squares.G1.AsBit() : BoardSquares.Squares.G8.AsBit();

    internal static ulong KingLongCastlePosition(bool isWhite) =>
        isWhite ? BoardSquares.Squares.C1.AsBit() : BoardSquares.Squares.C8.AsBit();



    internal struct Boxes
    {

        internal static ulong A1G7 { get; } = 0x7f7f7f7f7f7f7f00;
        internal static ulong A2G8 { get; } = 0x7f7f7f7f7f7f7f;
        internal static ulong B1H7 { get; } = 0xfefefefefefefe00;
        internal static ulong B2H8 { get; } = 0xfefefefefefefe;
        internal static ulong A1G6 = 0x7f7f7f7f7f7f0000;
        internal static ulong A1F7 = 0x3f3f3f3f3f3f3f00;
        internal static ulong B1H6 = 0xfefefefefefe0000;
        internal static ulong A2F8 = 0x3f3f3f3f3f3f3f;
        internal static ulong A3G8 = 0x7f7f7f7f7f7f;
        internal static ulong C1H7 = 0xfcfcfcfcfcfcfc00;
        internal static ulong C2H8 = 0xfcfcfcfcfcfcfc;

        internal static ulong B3H8 = 0xfefefefefefe;
    }


}