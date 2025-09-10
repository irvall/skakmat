namespace skakmat.Game;
class Piece
{
    internal const int EmptySquare = -1;
    internal const int WhitePawn = 0;
    internal const int WhiteRook = 1;
    internal const int WhiteKnight = 2;
    internal const int WhiteBishop = 3;
    internal const int WhiteQueen = 4;
    internal const int WhiteKing = 5;
    internal const int BlackPawn = 6;
    internal const int BlackRook = 7;
    internal const int BlackKnight = 8;
    internal const int BlackBishop = 9;
    internal const int BlackQueen = 10;
    internal const int BlackKing = 11;

    internal static bool IsWhiteIndex(int pieceIndex)
    {
        return pieceIndex >= WhitePawn && pieceIndex < BlackPawn;
    }

    internal static int GetPieceIndex(PieceType type, BoardState state) => GetPieceIndex(type, state.WhiteToPlay);

    internal static int GetPieceIndex(PieceType type, bool isWhite) => type switch
    {
        PieceType.Pawn => isWhite ? WhitePawn : BlackPawn,
        PieceType.Rook => isWhite ? WhiteRook : BlackRook,
        PieceType.Knight => isWhite ? WhiteKnight : BlackKnight,
        PieceType.Bishop => isWhite ? WhiteBishop : BlackBishop,
        PieceType.Queen => isWhite ? WhiteQueen : BlackQueen,
        PieceType.King => isWhite ? WhiteKing : BlackKing,
        _ => throw new NotImplementedException(),
    };

    internal static bool IsCorrectColor(int pieceIndex, bool isWhite)
    {
        return isWhite ?
            pieceIndex is >= WhitePawn and <= WhiteKing
            : pieceIndex is >= BlackPawn and <= BlackKing;
    }

}