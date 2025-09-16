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

    internal static PieceType GetTypeFromIndex(int pieceIndex) => pieceIndex switch
    {
        WhitePawn or BlackPawn => PieceType.Pawn,
        WhiteRook or BlackRook => PieceType.Rook,
        WhiteKnight or BlackKnight => PieceType.Knight,
        WhiteBishop or BlackBishop => PieceType.Bishop,
        WhiteQueen or BlackQueen => PieceType.Queen,
        WhiteKing or BlackKing => PieceType.King,
        EmptySquare => throw new ArgumentException("Empty square has no piece type"),
        _ => throw new ArgumentException("Unknown pieceIndex: " + pieceIndex)
    };

    internal static bool IsCorrectColor(int pieceIndex, bool isWhite)
    {
        return isWhite ?
            pieceIndex is >= WhitePawn and <= WhiteKing
            : pieceIndex is >= BlackPawn and <= BlackKing;
    }

    internal static char PieceIndexAscii(int pieceIndex) => pieceIndex switch
    {
        WhitePawn => 'P',
        BlackPawn => 'p',
        WhiteRook => 'R',
        BlackRook => 'r',
        WhiteKnight => 'N',
        BlackKnight => 'n',
        WhiteBishop => 'B',
        BlackBishop => 'b',
        WhiteQueen => 'Q',
        BlackQueen => 'q',
        WhiteKing => 'K',
        BlackKing => 'k',
        EmptySquare => '.',
        _ => throw new ArgumentException("Unknown pieceIndex: " + pieceIndex)
    };

    internal static char PieceIndexUnicode(int pieceIndex) => pieceIndex switch
    {
        WhitePawn => '♟',
        BlackPawn => '♙',
        WhiteRook => '♜',
        BlackRook => '♖',
        WhiteKnight => '♞',
        BlackKnight => '♘',
        WhiteBishop => '♝',
        BlackBishop => '♗',
        WhiteQueen => '♛',
        BlackQueen => '♕',
        WhiteKing => '♚',
        BlackKing => '♔',
        EmptySquare => '.',
        _ => throw new ArgumentException("Unknown pieceIndex: " + pieceIndex)
    };



}