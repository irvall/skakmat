using static BoardDatastructures;

namespace chess_rts;

internal static class BoardHelpers
{
    public static Board RandomBoard()
    {
        var board = new Board(512, 512);
        var rng = new Random();
        const double chanceOfDrop = 0.3;

        for (var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
            {
                if (rng.NextDouble() > chanceOfDrop) continue;
                var isWhite = rng.NextDouble() < 0.5;
                var index = y * 8 + x;
                Console.WriteLine($"Placing piece at {x}, {y} - index {index}");
                switch (rng.NextInt64(6))
                {
                    case 0:
                        {
                            if (y is 0 or 7) break;
                            if (isWhite) Board.SetBit(ref board.WhitePawns, x, y);
                            else Board.SetBit(ref board.BlackPawns, x, y);
                            break;
                        }
                    case 1:
                        {
                            if (isWhite) Board.SetBit(ref board.WhiteKnights, x, y);
                            else Board.SetBit(ref board.BlackKnights, x, y);
                            break;
                        }
                    case 2:
                        {
                            if (isWhite) Board.SetBit(ref board.WhiteBishops, x, y);
                            else Board.SetBit(ref board.BlackBishops, x, y);
                            break;
                        }
                    case 3:
                        {
                            if (isWhite) Board.SetBit(ref board.WhiteRooks, x, y);
                            else Board.SetBit(ref board.BlackRooks, x, y);
                            break;
                        }
                    case 4:
                        {
                            if (isWhite) Board.SetBit(ref board.WhiteQueens, x, y);
                            else Board.SetBit(ref board.BlackQueens, x, y);
                            break;
                        }
                    case 5:
                        {
                            if (isWhite) Board.SetBit(ref board.WhiteKings, x, y);
                            else Board.SetBit(ref board.BlackKings, x, y);
                            break;
                        }
                }
            }


        return board;
    }

    public static Board FromFen(string fen, bool laptop = false)
    {
        var board = new Board(laptop ? 600 : 1024, laptop ? 600 : 1024);
        var parts = fen.Split(' ');
        var rows = parts[0].Split('/');
        var row = 0;
        var col = 0;
        foreach (var r in rows)
        {
            foreach (var c in r)
            {
                if (char.IsDigit(c))
                {
                    col += int.Parse(c.ToString());
                    continue;
                }

                switch (c)
                {
                    case 'K':
                        board.WhiteKings |= 1UL << (row * Board.SquareNo + col);
                        break;
                    case 'Q':
                        board.WhiteQueens |= 1UL << (row * Board.SquareNo + col);
                        break;
                    case 'R':
                        board.WhiteRooks |= 1UL << (row * Board.SquareNo + col);
                        break;
                    case 'B':
                        board.WhiteBishops |= 1UL << (row * Board.SquareNo + col);
                        break;
                    case 'N':
                        board.WhiteKnights |= 1UL << (row * Board.SquareNo + col);
                        break;
                    case 'P':
                        board.WhitePawns |= 1UL << (row * Board.SquareNo + col);
                        break;
                    case 'k':
                        board.BlackKings |= 1UL << (row * Board.SquareNo + col);
                        break;
                    case 'q':
                        board.BlackQueens |= 1UL << (row * Board.SquareNo + col);
                        break;
                    case 'r':
                        board.BlackRooks |= 1UL << (row * Board.SquareNo + col);
                        break;
                    case 'b':
                        board.BlackBishops |= 1UL << (row * Board.SquareNo + col);
                        break;
                    case 'n':
                        board.BlackKnights |= 1UL << (row * Board.SquareNo + col);
                        break;
                    case 'p':
                        board.BlackPawns |= 1UL << (row * Board.SquareNo + col);
                        break;
                }

                col++;
            }

            row++;
            col = 0;
        }

        board.InitialState = new BoardState(board.WhiteKings, board.WhiteQueens, board.WhiteRooks, board.WhiteBishops,
            board.WhiteKnights, board.WhitePawns, board.BlackKings, board.BlackQueens, board.BlackRooks,
            board.BlackBishops, board.BlackKnights, board.BlackPawns);
        return board;
    }
}