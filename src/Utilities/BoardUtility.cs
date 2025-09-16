using System.Numerics;
using skakmat.Game;

namespace skakmat.Utilities;

internal class BoardUtility
{

    internal static ulong[] BitboardFromFen(string fen)
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
                            var idx = isWhite ? Piece.WhitePawn : Piece.BlackPawn;
                            bbs[idx] |= bit;
                            break;
                        }
                    case 'r':
                        {
                            var idx = isWhite ? Piece.WhiteRook : Piece.BlackRook;
                            bbs[idx] |= bit;
                            break;
                        }
                    case 'n':
                        {
                            var idx = isWhite ? Piece.WhiteKnight : Piece.BlackKnight;
                            bbs[idx] |= bit;
                            break;
                        }
                    case 'b':
                        {
                            var idx = isWhite ? Piece.WhiteBishop : Piece.BlackBishop;
                            bbs[idx] |= bit;
                            break;
                        }
                    case 'q':
                        {
                            var idx = isWhite ? Piece.WhiteQueen : Piece.BlackQueen;
                            bbs[idx] |= bit;
                            break;
                        }
                    case 'k':
                        {
                            var idx = isWhite ? Piece.WhiteKing : Piece.BlackKing;
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

    internal static IEnumerable<(int index, ulong bit)> EnumerateSquares()
    {
        for (var idx = 0; idx < 64; idx++)
        {
            var bit = 1UL << idx;
            yield return (idx, bit);
        }
    }

    internal static int IndexUnderMouse(Vector2 mousePosition, bool boardStandardOrientation = true)
    {
        var index = (int)(mousePosition.X + mousePosition.Y * Constants.SquareCount);
        return boardStandardOrientation ? index : 63 - index;
    }

    internal static (int, ulong) IndexAndBitUnderMouse(Vector2 mousePosition)
    {
        var idx = IndexUnderMouse(mousePosition);
        return (idx, 1UL << idx);
    }

    internal static string IndexToSquareString(int index)
    {
        var letter = (char)('a' + (index % 8));
        return letter + (8 - (index / 8)).ToString();

    }

    internal static int BitToIndex(ulong bit)
    {
        foreach (var (idx, squareBit) in EnumerateSquares())
            if (bit.Contains(squareBit))
                return idx;
        return -1;
    }

    internal static string PieceIndexToString(int pieceIndex)
    {
        return pieceIndex switch
        {
            Piece.WhitePawn or Piece.BlackPawn => "",
            Piece.WhiteRook or Piece.BlackRook => "R",
            Piece.WhiteKnight or Piece.BlackKnight => "N",
            Piece.WhiteBishop or Piece.BlackBishop => "B",
            Piece.WhiteQueen or Piece.BlackQueen => "Q",
            Piece.WhiteKing or Piece.BlackKing => "K",
            _ => throw new NotImplementedException(),
        };
    }

    internal static Move InvertMove(Move move)
    {
        return new Move(move.PieceIndex, move.TargetBit, move.OriginBit);
    }

    internal static bool AreHorizontalNeighbors(ulong bit1, ulong bit2)
    {
        int pos1 = BitOperations.TrailingZeroCount(bit1);
        int pos2 = BitOperations.TrailingZeroCount(bit2);

        // Test if same rank
        if (pos1 / 8 != pos2 / 8) return false;

        // Test if adjacent files
        return Math.Abs(pos1 - pos2) == 1;
    }

    internal static void PrintPrettyBoard(BoardState boardState)
    {
        Console.WriteLine();
        for (var i = 0; i < 64; i++)
        {
            var pieceIdx = boardState.GetPieceIndexAtIndex(i);
            Console.Write(Piece.PieceIndexUnicode(pieceIdx));

            if ((i + 1) % 8 == 0)
                Console.WriteLine();
            else
                Console.Write('|');
        }
        Console.WriteLine();
    }

    internal static void PrintMoveHistory(List<Move> moves)
    {
        Console.Clear();
        Console.WriteLine("Move History:");
        for (var i = 0; i < moves.Count; i += 2)
        {
            if (i + 1 >= moves.Count) break;
            var isCurrentMove = i == moves.Count - 2;
            if (isCurrentMove) Console.ForegroundColor = ConsoleColor.Yellow;
            else Console.ResetColor();
            Console.WriteLine($"{(i / 2) + 1}. {moves[i].ToSanNotation()} {moves[i + 1].ToSanNotation()}");
        }
        var isCurrentLastMove = moves.Count % 2 != 0;
        if (isCurrentLastMove) Console.ForegroundColor = ConsoleColor.Yellow;
        else Console.ResetColor();
        if (moves.Count % 2 != 0)
        {
            Console.WriteLine($"{(moves.Count / 2) + 1}. {moves[^1].ToSanNotation()}");
        }
        Console.ResetColor();
    }



}