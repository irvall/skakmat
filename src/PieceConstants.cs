namespace skakmat;
public static class PieceConstants
{
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

}