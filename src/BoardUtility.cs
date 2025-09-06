using System.Numerics;

namespace skakmat;

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

    public static (int, ulong) IndexAndBitUnderMouse(Vector2 mousePosition)
    {
        var idx = (int)(mousePosition.X + mousePosition.Y * Constants.SquareCount);
        return (idx, 1UL << idx);
    }

}