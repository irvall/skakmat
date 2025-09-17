using skakmat.Chess;
using skakmat.Extensions;

namespace skakmat.Game;
internal class Castling
{
    internal enum Type
    {
        None,
        KingSide,
        QueenSide
    }

    [Flags]
    internal enum Rights
    {
        None = 0,
        WhiteKingSide = 1,
        WhiteQueenSide = 2,
        BlackKingSide = 4,
        BlackQueenSide = 8,
        All = 15
    }

    internal static Type GetCastlingType(bool whiteToPlay, ulong targetBit, Position position)
    {
        if (Masks.KingAttemptsShortCastle(whiteToPlay).Contains(targetBit))
        {
            var requiredRight = whiteToPlay ? Rights.WhiteKingSide : Rights.BlackKingSide;
            return position.CastlingRights.HasFlag(requiredRight) ? Type.KingSide : Type.None;
        }

        if (Masks.KingAttemptsLongCastle(whiteToPlay).Contains(targetBit))
        {
            var requiredRight = whiteToPlay ? Rights.WhiteQueenSide : Rights.BlackQueenSide;
            return position.CastlingRights.HasFlag(requiredRight) ? Type.QueenSide : Type.None;
        }

        return Type.None;
    }

    internal static Move CreateRookMove(Type type, bool whiteToPlay)
    {
        var rookType = whiteToPlay ? Piece.WhiteRook : Piece.BlackRook;
        if (type == Type.KingSide)
        {
            var originBit = Masks.RookRightCorner(whiteToPlay);
            var targetBit = Masks.RookShortCastlePosition(whiteToPlay);
            return new Move(rookType, originBit, targetBit);
        }
        else if (type == Type.QueenSide)
        {
            var originBit = Masks.RookLeftCorner(whiteToPlay);
            var targetBit = Masks.RookLongCastlePosition(whiteToPlay);
            return new Move(rookType, originBit, targetBit);
        }
        throw new ArgumentException("Castling type unexpected: " + type);
    }

    internal static Move CreateKingMove(Type type, bool whiteToPlay)
    {
        var kingType = whiteToPlay ? Piece.WhiteKing : Piece.BlackKing;
        var originBit = Masks.KingStartSquare(whiteToPlay);
        if (type == Type.KingSide)
        {
            var targetBit = Masks.KingShortCastlePosition(whiteToPlay);
            return new Move(kingType, originBit, targetBit);
        }
        else if (type == Type.QueenSide)
        {
            var targetBit = Masks.KingLongCastlePosition(whiteToPlay);
            return new Move(kingType, originBit, targetBit);
        }
        throw new ArgumentException("Castling type unexpected: " + type);
    }

}