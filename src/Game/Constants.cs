namespace skakmat.Game;
internal static class Constants
{
    internal static readonly Dictionary<int, (int, int)> PieceToSpriteCoords = new()
    {
        { Piece.WhitePawn, (5, 0) },
        { Piece.WhiteKnight, (3, 0) },
        { Piece.WhiteBishop, (2, 0) },
        { Piece.WhiteRook, (4, 0) },
        { Piece.WhiteQueen, (1, 0) },
        { Piece.WhiteKing, (0, 0) },
        { Piece.BlackPawn, (5, 1) },
        { Piece.BlackKnight, (3, 1) },
        { Piece.BlackBishop, (2, 1) },
        { Piece.BlackRook, (4, 1) },
        { Piece.BlackQueen, (1, 1) },
        { Piece.BlackKing, (0, 1) },
    };
    internal const int SquareCount = 8;

    internal static class FenPositions
    {
        internal const string Default = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        internal const string RookCheck = "8/5pk1/p4pp1/2p4p/2P1P2P/1P3PP1/P2r2K1/4R3 w - - 0 40";
        internal const string MaxPieceDensity = "rnbqkb1r/pp1p1ppp/2p5/4P3/2B5/8/PPP1NnPP/RNBQK2R w KQkq - 0 6";
        internal const string Nasty = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 b kq a3 5 17";
        internal const string WhiteToSmother = "rnb3rk/pppp2pp/8/2q3Np/8/8/PPPPPPPP/RNBQKB1R w KQq - 0 1";
        internal const string WhiteKingInCheck1 = "b3r3/8/8/8/8/4Kn2/8/1B6 w - - 0 1";
        internal const string WhiteKingCanCastle = "7k/8/8/2b5/8/8/5PPP/4K2R w K - 0 1";
        internal const string BothSidesCanCastle = "rnbqk2r/pppp1ppp/5n2/2b1p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4";
        internal const string BlackForcedToTake = "4n2k/8/8/7N/8/8/8/K5Q1 w - - 0 1";
        internal const string PinnedMate = "rnbqkb1r/pppppp1p/8/8/4Q1N1/8/PPPPPPPP/RNB1KB1R w KQkq - 0 1";
    }

}