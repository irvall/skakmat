using skakmat.Chess;
using skakmat.Game;

namespace skakmat.Utilities;
public static class MoveExtensions
{

    public static string ToLanNotation(this Move move)
    {
        var fromIndex = BoardUtility.BitToIndex(move.OriginBit);
        var toIndex = BoardUtility.BitToIndex(move.TargetBit);
        var pieceTypeString = BoardUtility.PieceTypeToString(move.PieceType);
        return $"{pieceTypeString}{BoardUtility.IndexToSquareString(fromIndex)}{BoardUtility.IndexToSquareString(toIndex)}";
    }

    public static string ToSanNotation(this Move move)
    {
        var toIndex = BoardUtility.BitToIndex(move.TargetBit);
        var pieceTypeString = BoardUtility.PieceTypeToString(move.PieceType);
        return $"{pieceTypeString}{BoardUtility.IndexToSquareString(toIndex)}";
    }

    public static ulong ToBitboard(this List<Move> moves)
    {
        var bitboard = 0UL;
        moves.ForEach(move =>
        {
            bitboard |= move.TargetBit;
        });
        return bitboard;
    }

    public static bool IsShortCastle(this Move move)
    {
        var whiteCastle = move.PieceType == Constants.WhiteKing
            && move.OriginBit == Masks.KingStartSquare(isWhite: true)
            && Masks.KingAttemptsShortCastle(isWhite: true).Contains(move.TargetBit);
        var blackCastle = move.PieceType == Constants.BlackKing
            && move.OriginBit == Masks.KingStartSquare(isWhite: false)
            && Masks.KingAttemptsShortCastle(isWhite: false).Contains(move.TargetBit);
        return whiteCastle || blackCastle;
    }

    public static bool IsLongCastle(this Move move)
    {
        var whiteCastle = move.PieceType == Constants.WhiteKing
            && move.OriginBit == Masks.KingStartSquare(isWhite: true)
            && Masks.KingAttemptsLongCastle(isWhite: true).Contains(move.TargetBit);
        var blackCastle = move.PieceType == Constants.BlackKing
            && move.OriginBit == Masks.KingStartSquare(isWhite: false)
            && Masks.KingAttemptsLongCastle(isWhite: false).Contains(move.TargetBit);
        return whiteCastle || blackCastle;
    }

}