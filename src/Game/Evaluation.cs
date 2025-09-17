using skakmat.Helpers;
namespace skakmat.Game;
public class Evaluation
{

    private static readonly Dictionary<PieceType, float> Weights = new()
    {
        { PieceType.King, 200 },
        { PieceType.Queen, 9 },
        { PieceType.Rook, 5 },
        { PieceType.Bishop, 3.5f },
        { PieceType.Knight, 3f },
        { PieceType.Pawn, 1 }
    };

    internal static float EvaluatePosition(Position position)
    {
        var whiteSide = 0.0f;
        var blackSide = 0.0f;
        var pieceCounts = BoardHelper.GetNumberOfPieces(position);
        foreach (var (idx, count) in pieceCounts.ToList())
        {
            if (idx == Piece.EmptySquare) continue;
            var type = Piece.GetTypeFromIndex(idx);
            var weight = Weights[type];
            if (Piece.IsWhiteIndex(idx))
                whiteSide += count * weight;
            else
                blackSide += count * weight;
        }
        return whiteSide - blackSide;
    }

}