class BoardDatastructures
{
    internal static class Masks
    {
        public const ulong FileA = 0x0101010101010101UL;
        public const ulong FileB = FileA << 1;
        public const ulong FileC = FileA << 2;
        public const ulong FileD = FileA << 3;
        public const ulong FileE = FileA << 4;
        public const ulong FileF = FileA << 5;
        public const ulong FileG = FileA << 6;
        public const ulong FileH = FileA << 7;
        public const ulong Rank1 = 0xFF00000000000000UL;
        public const ulong Rank2 = Rank1 >> 8;
        public const ulong Rank3 = Rank1 >> 16;
        public const ulong Rank4 = Rank1 >> 24;
        public const ulong Rank5 = Rank1 >> 32;
        public const ulong Rank6 = Rank1 >> 40;
        public const ulong Rank7 = Rank1 >> 48;
        public const ulong Rank8 = Rank1 >> 56;
        public const ulong DiagonalA1H8 = 0x8040201008040201UL;
        public const ulong DiagonalH1A8 = 0x0102040810204080UL;
    }

    public struct BoardState
    {
        public ulong WhiteKings;
        public ulong WhiteQueens;
        public ulong WhiteRooks;
        public ulong WhiteBishops;
        public ulong WhiteKnights;
        public ulong WhitePawns;
        public ulong BlackKings;
        public ulong BlackQueens;
        public ulong BlackRooks;
        public ulong BlackBishops;
        public ulong BlackKnights;
        public ulong BlackPawns;
        public BoardState(ulong WhiteKings, ulong WhiteQueens, ulong WhiteRooks, ulong WhiteBishops, ulong WhiteKnights, ulong WhitePawns, ulong BlackKings, ulong BlackQueens, ulong BlackRooks, ulong BlackBishops, ulong BlackKnights, ulong BlackPawns)
        {
            this.WhiteKings = WhiteKings;
            this.WhiteQueens = WhiteQueens;
            this.WhiteRooks = WhiteRooks;
            this.WhiteBishops = WhiteBishops;
            this.WhiteKnights = WhiteKnights;
            this.WhitePawns = WhitePawns;
            this.BlackKings = BlackKings;
            this.BlackQueens = BlackQueens;
            this.BlackRooks = BlackRooks;
            this.BlackBishops = BlackBishops;
            this.BlackKnights = BlackKnights;
            this.BlackPawns = BlackPawns;
        }

    }

    internal enum PieceColor
    {
        White,
        Black
    };

    internal enum PieceType
    {
        WhiteKing,
        WhiteQueen,
        WhiteBishop,
        WhiteKnight,
        WhiteRook,
        WhitePawn,
        BlackKing,
        BlackQueen,
        BlackBishop,
        BlackKnight,
        BlackRook,
        BlackPawn
    };
}