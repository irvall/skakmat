namespace skakmat;
public static class Constants
{
    public const int EmptySquare = -1;
    public const int WhitePawn = 0;
    public const int WhiteRook = 1;
    public const int WhiteKnight = 2;
    public const int WhiteBishop = 3;
    public const int WhiteQueen = 4;
    public const int WhiteKing = 5;
    public const int BlackPawn = 6;
    public const int BlackRook = 7;
    public const int BlackKnight = 8;
    public const int BlackBishop = 9;
    public const int BlackQueen = 10;
    public const int BlackKing = 11;

    public static readonly Dictionary<int, (int, int)> PieceToSpriteCoords = new()
    {
        { WhitePawn, (5, 0) },
        { WhiteKnight, (3, 0) },
        { WhiteBishop, (2, 0) },
        { WhiteRook, (4, 0) },
        { WhiteQueen, (1, 0) },
        { WhiteKing, (0, 0) },
        { BlackPawn, (5, 1) },
        { BlackKnight, (3, 1) },
        { BlackBishop, (2, 1) },
        { BlackRook, (4, 1) },
        { BlackQueen, (1, 1) },
        { BlackKing, (0, 1) },
    };
    public const int SquareCount = 8;
    public const string FenDefaultPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public const string FenRookCheck = "8/5pk1/p4pp1/2p4p/2P1P2P/1P3PP1/P2r2K1/4R3 w - - 0 40";
    public const string FenMaxPieceDensity = "rnbqkb1r/pp1p1ppp/2p5/4P3/2B5/8/PPP1NnPP/RNBQK2R w KQkq - 0 6";
    public const string FenNasty = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 b kq a3 5 17";

}