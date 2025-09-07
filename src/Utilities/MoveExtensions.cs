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

}