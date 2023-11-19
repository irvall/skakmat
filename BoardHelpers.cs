class BoardHelpers
{
    public Board RandomBoard()
    {
        var board = new Board(512, 512);
        var rng = new Random();
        var chanceOfDrop = 0.3;

        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                if (rng.NextDouble() > chanceOfDrop) continue;
                var isWhite = rng.NextDouble() < 0.5;
                var index = y * 8 + x;
                System.Console.WriteLine($"Placing piece at {x}, {y} - index {index}");
                switch (rng.NextInt64(6))
                {
                    case 0:
                        {
                            if (y == 0 || y == 7) break;
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
}
