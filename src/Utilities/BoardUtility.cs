using System.Numerics;
using skakmat.Game;

namespace skakmat.Utilities;

public class BoardUtility
{

    public static ulong[] BitboardFromFen(string fen)
    {
        // Standard: rnbqkbnr/pppppppp/Constants.SquareCount/Constants.SquareCount/Constants.SquareCount/Constants.SquareCount/PPPPPPPP/RNBQKBNR w KQkq - 0 1
        // TODO: Support 'w KQkq - 0 1' part
        var bbs = new ulong[12];
        var parts = fen.Split(' ');
        var position = parts[0];
        var slashCount = position.Count(ch => ch == '/');
        if (slashCount != 7)
        {
            throw new FormatException("Expected 7 '/' in a FEN string: " + fen);
        }
        var rows = position.Split('/');
        for (var row = 0; row < Constants.SquareCount; row++)
        {
            var currentRow = rows[row];
            var ri = 0;
            var i = 0;
            while (ri < currentRow.Length)
            {
                var bit = 1UL << i << row * Constants.SquareCount;
                var ch = currentRow[ri];
                var isWhite = char.IsUpper(ch);
                switch (char.ToLower(ch))
                {
                    case 'p':
                        {
                            var idx = isWhite ? Constants.WhitePawn : Constants.BlackPawn;
                            bbs[idx] |= bit;
                            break;
                        }
                    case 'r':
                        {
                            var idx = isWhite ? Constants.WhiteRook : Constants.BlackRook;
                            bbs[idx] |= bit;
                            break;
                        }
                    case 'n':
                        {
                            var idx = isWhite ? Constants.WhiteKnight : Constants.BlackKnight;
                            bbs[idx] |= bit;
                            break;
                        }
                    case 'b':
                        {
                            var idx = isWhite ? Constants.WhiteBishop : Constants.BlackBishop;
                            bbs[idx] |= bit;
                            break;
                        }
                    case 'q':
                        {
                            var idx = isWhite ? Constants.WhiteQueen : Constants.BlackQueen;
                            bbs[idx] |= bit;
                            break;
                        }
                    case 'k':
                        {
                            var idx = isWhite ? Constants.WhiteKing : Constants.BlackKing;
                            bbs[idx] |= bit;
                            break;
                        }
                }
                if (char.IsNumber(ch))
                {
                    i += (int)char.GetNumericValue(ch);
                }
                else
                {
                    i++;
                }
                ri++;
            }

        }
        return bbs;
    }

    public static IEnumerable<(int index, ulong bit)> EnumerateSquares()
    {
        for (var idx = 0; idx < 64; idx++)
        {
            var bit = 1UL << idx;
            yield return (idx, bit);
        }
    }

    public static (int, ulong) IndexAndBitUnderMouse(Vector2 mousePosition)
    {
        var idx = (int)(mousePosition.X + mousePosition.Y * Constants.SquareCount);
        return (idx, 1UL << idx);
    }

    public static string IndexToSquareString(int index)
    {
        var letter = (char)('a' + (index % 8));
        return letter + (8 - (index / 8)).ToString();

    }

    public static int BitToIndex(ulong bit)
    {
        foreach (var (idx, squareBit) in EnumerateSquares())
            if (bit.Contains(squareBit))
                return idx;
        return -1;
    }

    public static string PieceTypeToString(int pieceType)
    {
        return pieceType switch
        {
            Constants.WhitePawn or Constants.BlackPawn => "",
            Constants.WhiteRook or Constants.BlackRook => "R",
            Constants.WhiteKnight or Constants.BlackKnight => "N",
            Constants.WhiteBishop or Constants.BlackBishop => "B",
            Constants.WhiteQueen or Constants.BlackQueen => "Q",
            Constants.WhiteKing or Constants.BlackKing => "K",
            _ => throw new NotImplementedException(),
        };
    }

    public static Move InvertMove(Move move)
    {
        return new Move(move.PieceType, move.TargetBit, move.OriginBit);
    }

    public static bool AreHorizontalNeighbors(ulong bit1, ulong bit2)
    {
        int pos1 = BitOperations.TrailingZeroCount(bit1);
        int pos2 = BitOperations.TrailingZeroCount(bit2);

        // Test if same rank
        if (pos1 / 8 != pos2 / 8) return false;

        // Test if adjacent files
        return Math.Abs(pos1 - pos2) == 1;
    }

}