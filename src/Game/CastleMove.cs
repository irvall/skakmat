namespace skakmat.Game;

internal class CastleMove(Move kingMove, Move rookMove) : Move(kingMove.PieceIndex, kingMove.OriginBit, kingMove.TargetBit)
{
    internal Move RookMove { get; } = rookMove;
}