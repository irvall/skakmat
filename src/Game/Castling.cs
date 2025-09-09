using skakmat.Chess;
using skakmat.Utilities;

namespace skakmat.Game;
public class Castling
{
    public enum Type
    {
        None,
        KingSide,
        QueenSide
    }

    [Flags]
    public enum Rights
    {
        None = 0,
        WhiteKingSide = 1,
        WhiteQueenSide = 2,
        BlackKingSide = 4,
        BlackQueenSide = 8,
        All = 15
    }

    public static Type GetCastlingType(bool whiteToPlay, ulong targetBit, Rights rights)
    {
        if (Masks.KingAttemptsShortCastle(whiteToPlay).Contains(targetBit))
        {
            var requiredRight = whiteToPlay ? Rights.WhiteKingSide : Rights.BlackKingSide;
            return rights.HasFlag(requiredRight) ? Type.KingSide : Type.None;
        }

        if (Masks.KingAttemptsLongCastle(whiteToPlay).Contains(targetBit))
        {
            var requiredRight = whiteToPlay ? Rights.WhiteQueenSide : Rights.BlackQueenSide;
            return rights.HasFlag(requiredRight) ? Type.QueenSide : Type.None;
        }

        return Type.None;
    }

    public static Move CreateRookMove(Type type, bool whiteToPlay)
    {
        var rookType = whiteToPlay ? Constants.WhiteRook : Constants.BlackRook;
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

    public static Move CreateKingMove(Type type, bool whiteToPlay)
    {
        var kingType = whiteToPlay ? Constants.WhiteKing : Constants.BlackKing;
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