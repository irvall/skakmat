using System.Diagnostics.CodeAnalysis;
using skakmat.Chess;
using skakmat.Extensions;

namespace skakmat.Game;
internal class Move(int pieceIndex, ulong originBit, ulong targetBit)
{
    internal readonly int PieceIndex = pieceIndex;
    internal readonly ulong OriginBit = originBit;
    internal readonly ulong TargetBit = targetBit;

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Move moveObj && PieceIndex == moveObj.PieceIndex && OriginBit == moveObj.OriginBit && TargetBit == moveObj.TargetBit;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PieceIndex, OriginBit, TargetBit);
    }

    public static bool operator ==(Move left, Move right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Move left, Move right)
    {
        return !(left == right);
    }

    internal bool IsPawnDoublePush()
    {
        if (PieceIndex != Piece.WhitePawn
            && PieceIndex != Piece.BlackPawn)
            return false;
        var startRank = this.IsWhite() ? Masks.Rank2 : Masks.Rank7;
        var targetRank = this.IsWhite() ? Masks.Rank4 : Masks.Rank5;
        return startRank.Contains(OriginBit) && targetRank.Contains(TargetBit);
    }

    internal Move TryCreateCastleMove(bool whiteToPlay)
    {
        Castling.Type type = Castling.Type.None;
        if (this.IsShortCastle())
            type = Castling.Type.KingSide;
        else if (this.IsLongCastle())
            type = Castling.Type.QueenSide;
        if (type == Castling.Type.None)
            return this;
        var rookMove = Castling.CreateRookMove(type, whiteToPlay);
        var kingMove = Castling.CreateKingMove(type, whiteToPlay);
        return new CastleMove(kingMove, rookMove);
    }

}