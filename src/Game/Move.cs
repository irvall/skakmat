using System.Diagnostics.CodeAnalysis;

namespace skakmat.Game;
public class Move(int pieceType, ulong originBit, ulong targetBit)
{
    public readonly int PieceType = pieceType;
    public readonly ulong OriginBit = originBit;
    public readonly ulong TargetBit = targetBit;

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Move moveObj && PieceType == moveObj.PieceType && OriginBit == moveObj.OriginBit && TargetBit == moveObj.TargetBit;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PieceType, OriginBit, TargetBit);
    }
    public static bool operator ==(Move left, Move right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Move left, Move right)
    {
        return !(left == right);
    }
}