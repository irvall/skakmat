namespace skakmat.Game;

public class EnPassantMove(Move pawnAttacking, Move pawnToRemove) : Move(pawnAttacking.PieceType, pawnAttacking.OriginBit, pawnAttacking.TargetBit)
{
    public Move PawnToRemove { get; } = pawnToRemove;
}