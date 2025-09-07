namespace skakmat.Game;
public readonly struct Move(int pieceType, ulong originBit, ulong targetBit)
{
    public readonly int PieceType = pieceType;
    public readonly ulong OriginBit = originBit;
    public readonly ulong TargetBit = targetBit;
}