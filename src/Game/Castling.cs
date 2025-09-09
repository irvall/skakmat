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

    public static Type GetCastlingType(bool whiteToPlay, ulong targetBit)
    {
        Type type = Type.None;
        if (Masks.KingAttemptsShortCastle(whiteToPlay).Contains(targetBit))
            type = Type.KingSide;
        else if (Masks.KingAttemptsLongCastle(whiteToPlay).Contains(targetBit))
            type = Type.QueenSide;
        return type;
    }

}