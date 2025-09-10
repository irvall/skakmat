using skakmat.Chess;
using skakmat.Game;

namespace skakmat.Utilities;
internal static class MoveExtensions
{

    internal static string ToLanNotation(this Move move)
    {
        var fromIndex = BoardUtility.BitToIndex(move.OriginBit);
        var toIndex = BoardUtility.BitToIndex(move.TargetBit);
        var pieceIndexString = BoardUtility.PieceIndexToString(move.PieceIndex);
        return $"{pieceIndexString}{BoardUtility.IndexToSquareString(fromIndex)}{BoardUtility.IndexToSquareString(toIndex)}";
    }

    internal static string ToSanNotation(this Move move)
    {
        var toIndex = BoardUtility.BitToIndex(move.TargetBit);
        var pieceIndexString = BoardUtility.PieceIndexToString(move.PieceIndex);
        return $"{pieceIndexString}{BoardUtility.IndexToSquareString(toIndex)}";
    }

    internal static ulong ToBitboard(this List<Move> moves)
    {
        var bitboard = 0UL;
        moves.ForEach(move =>
        {
            bitboard |= move.TargetBit;
        });
        return bitboard;
    }

    internal static bool IsShortCastle(this Move move)
    {
        var whiteCastle = move.PieceIndex == Piece.WhiteKing
            && move.OriginBit == Masks.KingStartSquare(isWhite: true)
            && Masks.KingAttemptsShortCastle(isWhite: true).Contains(move.TargetBit);
        var blackCastle = move.PieceIndex == Piece.BlackKing
            && move.OriginBit == Masks.KingStartSquare(isWhite: false)
            && Masks.KingAttemptsShortCastle(isWhite: false).Contains(move.TargetBit);
        return whiteCastle || blackCastle;
    }

    internal static bool IsWhite(this Move move)
    {
        return Piece.IsWhiteIndex(move.PieceIndex);
    }

    internal static bool IsLongCastle(this Move move)
    {
        var isWhite = move.IsWhite();
        var whiteCastle = move.PieceIndex == Piece.WhiteKing
            && move.OriginBit == Masks.KingStartSquare(isWhite: true)
            && Masks.KingAttemptsLongCastle(isWhite: true).Contains(move.TargetBit);
        var blackCastle = move.PieceIndex == Piece.BlackKing
            && move.OriginBit == Masks.KingStartSquare(isWhite: false)
            && Masks.KingAttemptsLongCastle(isWhite: false).Contains(move.TargetBit);
        return whiteCastle || blackCastle;
    }

}