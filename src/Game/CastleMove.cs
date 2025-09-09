namespace skakmat.Game;

public class CastleMove(Move move) : Move(move.PieceType, move.OriginBit, move.TargetBit)
{
    public required Move KingMove { get; set; }
    public required Move RookMove { get; set; }
}