

using skakmat.Game;
using skakmat.Helpers;

namespace skakmat.Extensions;

internal static class PositionExtensions
{

    internal static int GetPieceIndexAt(this Position position, ulong square)
    {
        if (position.Bitboards[Piece.WhitePawn].Contains(square)) return Piece.WhitePawn;
        if (position.Bitboards[Piece.BlackPawn].Contains(square)) return Piece.BlackPawn;
        if (position.Bitboards[Piece.WhiteRook].Contains(square)) return Piece.WhiteRook;
        if (position.Bitboards[Piece.BlackRook].Contains(square)) return Piece.BlackRook;
        if (position.Bitboards[Piece.WhiteKnight].Contains(square)) return Piece.WhiteKnight;
        if (position.Bitboards[Piece.BlackKnight].Contains(square)) return Piece.BlackKnight;
        if (position.Bitboards[Piece.WhiteBishop].Contains(square)) return Piece.WhiteBishop;
        if (position.Bitboards[Piece.BlackBishop].Contains(square)) return Piece.BlackBishop;
        if (position.Bitboards[Piece.WhiteQueen].Contains(square)) return Piece.WhiteQueen;
        if (position.Bitboards[Piece.BlackQueen].Contains(square)) return Piece.BlackQueen;
        if (position.Bitboards[Piece.WhiteKing].Contains(square)) return Piece.WhiteKing;
        if (position.Bitboards[Piece.BlackKing].Contains(square)) return Piece.BlackKing;
        return Piece.EmptySquare;
    }

    internal static int GetPieceIndexAt(this Position position, int squareIndex)
    {
        return position.GetPieceIndexAt(1UL << squareIndex);
    }

    internal static int GetPieceIndex(this Position position, PieceType type)
    {
        return Piece.GetPieceIndex(type, position.WhiteToPlay);
    }

    internal static ulong GetPieceBoard(this Position position, PieceType type)
    {
        var pieceIndex = position.GetPieceIndex(type);
        return position.Bitboards[pieceIndex];
    }

    internal static IEnumerable<(int pieceIndex, int index, ulong bit)> GetAllPieces(this Position position)
    {
        foreach (var (idx, bit) in BoardHelper.EnumerateSquares())
        {
            var type = position.GetPieceIndexAt(bit);
            if (type != Piece.EmptySquare)
                yield return (type, idx, bit);
        }
    }


}