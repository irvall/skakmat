namespace ChessMate;

public class Masks
{
    public static ulong FileA { get; } = 0x0101010101010101;
    public static ulong FileB { get; } = 0x0202020202020202;
    public static ulong FileC { get; } = 0x0404040404040404;
    public static ulong FileD { get; } = 0x0808080808080808;
    public static ulong FileE { get; } = 0x1010101010101010;
    public static ulong FileF { get; } = 0x2020202020202020;
    public static ulong FileG { get; } = 0x4040404040404040;
    public static ulong FileH { get; } = 0x8080808080808080;
    public static ulong Rank1 { get; } = 0xFF00000000000000;
    public static ulong Rank2 { get; } = 0x00FF000000000000;
    public static ulong Rank3 { get; } = 0x0000FF0000000000;
    public static ulong Rank4 { get; } = 0x000000FF00000000;
    public static ulong Rank5 { get; } = 0x00000000FF000000;
    public static ulong Rank6 { get; } = 0x0000000000FF0000;
    public static ulong Rank7 { get; } = 0x000000000000FF00;
    public static ulong Rank8 { get; } = 0x00000000000000FF;
    public static ulong Center { get; } = 0x0000001818000000;
    public static ulong Corners { get; } = 0x8100000000000081;
    public static ulong CornersAndCenter { get; } = 0x8100001818000081;
    public static ulong CornersAndCenterAndAdjacent { get; } = 0xFF000018181800FF;

    public struct Boxes
    {
        public static ulong A1G7 { get; } = 0x7f7f7f7f7f7f7f00;
        public static ulong A2G8 { get; } = 0x7f7f7f7f7f7f7f;
        public static ulong B1H7 { get; } = 0xfefefefefefefe00;
        public static ulong B2H8 { get; } = 0xfefefefefefefe;
        public static ulong A1G6 = 0x7f7f7f7f7f7f0000;
        public static ulong A1F7 = 0x3f3f3f3f3f3f3f00;
        public static ulong B1H6 = 0xfefefefefefe0000;
        public static ulong A2F8 = 0x3f3f3f3f3f3f3f;
        public static ulong A3G8 = 0x7f7f7f7f7f7f;
        public static ulong C1H7 = 0xfcfcfcfcfcfcfc00;
        public static ulong C2H8 = 0xfcfcfcfcfcfcfc;

        public static ulong B3H8 = 0xfefefefefefe;
    }
}