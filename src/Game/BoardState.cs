
using skakmat.Utilities;

namespace skakmat.Game;
internal readonly struct BoardState(ulong[] bitboards, bool whiteToPlay, Castling.Rights castlingRights, Move? lastMovePlayed)
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
    internal int GetPieceIndex(PieceType type) => Piece.GetPieceIndex(type, WhiteToPlay);

    internal int GetPieceIndexAtBit(ulong square)
    {
        if (Bitboards[Piece.WhitePawn].Contains(square)) return Piece.WhitePawn;
        if (Bitboards[Piece.BlackPawn].Contains(square)) return Piece.BlackPawn;
        if (Bitboards[Piece.WhiteRook].Contains(square)) return Piece.WhiteRook;
        if (Bitboards[Piece.BlackRook].Contains(square)) return Piece.BlackRook;
        if (Bitboards[Piece.WhiteKnight].Contains(square)) return Piece.WhiteKnight;
        if (Bitboards[Piece.BlackKnight].Contains(square)) return Piece.BlackKnight;
        if (Bitboards[Piece.WhiteBishop].Contains(square)) return Piece.WhiteBishop;
        if (Bitboards[Piece.BlackBishop].Contains(square)) return Piece.BlackBishop;
        if (Bitboards[Piece.WhiteQueen].Contains(square)) return Piece.WhiteQueen;
        if (Bitboards[Piece.BlackQueen].Contains(square)) return Piece.BlackQueen;
        if (Bitboards[Piece.WhiteKing].Contains(square)) return Piece.WhiteKing;
        if (Bitboards[Piece.BlackKing].Contains(square)) return Piece.BlackKing;
        return Piece.EmptySquare;

    }

    internal int GetPieceIndexAtIndex(int index)
    {
        return GetPieceIndexAtBit(1UL << index);
    }

    internal ulong GetPieceBoard(PieceType type)
    {
        var pieceIndex = GetPieceIndex(type);
        return Bitboards[pieceIndex];
    }
}