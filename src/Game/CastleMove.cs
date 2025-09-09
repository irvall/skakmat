namespace skakmat.Game;

public class CastleMove(Move kingMove, Move rookMove) : Move(kingMove.PieceType, kingMove.OriginBit, kingMove.TargetBit)
{
    public Move RookMove { get; } = rookMove;
}