namespace skakmat.Game;

internal class EnPassantMove(Move pawnAttacking, Move pawnToRemove) : Move(pawnAttacking.PieceIndex, pawnAttacking.OriginBit, pawnAttacking.TargetBit)
{
    internal Move PawnToRemove { get; } = pawnToRemove;
}