

namespace skakmat.Game;

internal readonly struct Position(ulong[] bitboards, bool whiteToPlay, Castling.Rights castlingRights, Move? lastMovePlayed)
{
    internal readonly ulong[] Bitboards = (ulong[])bitboards.Clone();
    internal readonly Castling.Rights CastlingRights = castlingRights;
    internal readonly Move? LastMovePlayed = lastMovePlayed;
    internal readonly bool WhiteToPlay => whiteToPlay;

    internal ulong WhitePieces =>
        Bitboards[Piece.WhitePawn] |
        Bitboards[Piece.WhiteKnight] |
        Bitboards[Piece.WhiteBishop] |
        Bitboards[Piece.WhiteRook] |
        Bitboards[Piece.WhiteQueen] |
        Bitboards[Piece.WhiteKing];

    internal ulong BlackPieces =>
        Bitboards[Piece.BlackPawn] |
        Bitboards[Piece.BlackKnight] |
        Bitboards[Piece.BlackBishop] |
        Bitboards[Piece.BlackRook] |
        Bitboards[Piece.BlackQueen] |
        Bitboards[Piece.BlackKing];

    internal ulong AllPieces => WhitePieces | BlackPieces;
    internal ulong EmptySquares => ~AllPieces;

    internal ulong GetFriendlyPieces() => WhiteToPlay ? WhitePieces : BlackPieces;
    internal ulong GetEnemyPieces() => WhiteToPlay ? BlackPieces : WhitePieces;

}