using System.Numerics;
using Raylib_cs;
using static BoardDatastructures;
using Color = Raylib_cs.Color;

class Board
{

    public MouseController MouseController { get; } = new MouseController();
    private Color boardBaseColor = Color.BROWN;

    private const int SquareNo = 8;

    private readonly int windowWidth;
    private readonly int windowHeight;

    private readonly int squareWidth;
    private readonly int squareHeight;

    private Texture2D pieceTexture;
    private int spriteWidth;
    private int spriteHeight;
    internal ulong WhiteKings = 0;
    internal ulong WhiteQueens = 0;
    internal ulong WhiteRooks = 0;
    internal ulong WhiteBishops = 0;
    internal ulong WhiteKnights = 0;
    internal ulong WhitePawns = 0;
    internal ulong BlackKings = 0;
    internal ulong BlackQueens = 0;
    internal ulong BlackRooks = 0;
    internal ulong BlackBishops = 0;
    internal ulong BlackKnights = 0;
    internal ulong BlackPawns = 0;

    private BoardState? InitialState = null;

    private ulong WhitePieces => WhiteKings | WhiteQueens | WhiteRooks | WhiteBishops | WhiteKnights | WhitePawns;
    private ulong BlackPieces => BlackKings | BlackQueens | BlackRooks | BlackBishops | BlackKnights | BlackPawns;
    public ulong AllPieces => WhitePieces | BlackPieces;
    private ulong EmptySquares => ~AllPieces;

    private ulong PieceSelected = 0UL;
    private PieceType? PieceSelectedType = null;

    private readonly Dictionary<ulong, (int row, int col)> possibleMoves = new();
    private readonly Dictionary<ulong, (int row, int col)> possibleCaptures = new();
    private static ulong GetBit(ulong bitBoard, int square) => bitBoard & (1UL << square);


    private BoardState[] history = new BoardState[100];
    private int historyIndex = 0;


    private void SaveBoardState()
    {
        if (historyIndex >= history.Length)
        {
            var newHistory = new BoardState[history.Length * 2];
            Array.Copy(history, newHistory, history.Length);
            history = newHistory;
        }
        history[historyIndex] = new BoardState(WhiteKings, WhiteQueens, WhiteRooks, WhiteBishops, WhiteKnights, WhitePawns, BlackKings, BlackQueens, BlackRooks, BlackBishops, BlackKnights, BlackPawns);
        historyIndex++;
    }

    internal static void SetBit(ref ulong bitboard, int x, int y)
    {
        var bit = 1UL << (y * 8 + x);
        bitboard |= bit;
    }

    private static void PrintBitBoard(ulong bitBoard, string optMessage = "")
    {
        Log.WriteColor(Log.BoldText(optMessage), ConsoleColor.Green);
        for (int y = 0; y < SquareNo; y++)
        {
            for (int x = 0; x < SquareNo; x++)
            {
                var idx = y * SquareNo + x;
                var theBit = GetBit(bitBoard, idx);
                if (theBit != 0)
                {
                    Console.Write(Log.BoldText("1 "));
                }
                else
                {
                    Log.WriteColor("0 ", ConsoleColor.DarkGray, false);
                }
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }


    public static Board FromFen(string fen)
    {
        var board = new Board(512, 512);
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
                        board.WhiteKings |= 1UL << (row * SquareNo + col);
                        break;
                    case 'Q':
                        board.WhiteQueens |= 1UL << (row * SquareNo + col);
                        break;
                    case 'R':
                        board.WhiteRooks |= 1UL << (row * SquareNo + col);
                        break;
                    case 'B':
                        board.WhiteBishops |= 1UL << (row * SquareNo + col);
                        break;
                    case 'N':
                        board.WhiteKnights |= 1UL << (row * SquareNo + col);
                        break;
                    case 'P':
                        board.WhitePawns |= 1UL << (row * SquareNo + col);
                        break;
                    case 'k':
                        board.BlackKings |= 1UL << (row * SquareNo + col);
                        break;
                    case 'q':
                        board.BlackQueens |= 1UL << (row * SquareNo + col);
                        break;
                    case 'r':
                        board.BlackRooks |= 1UL << (row * SquareNo + col);
                        break;
                    case 'b':
                        board.BlackBishops |= 1UL << (row * SquareNo + col);
                        break;
                    case 'n':
                        board.BlackKnights |= 1UL << (row * SquareNo + col);
                        break;
                    case 'p':
                        board.BlackPawns |= 1UL << (row * SquareNo + col);
                        break;
                }
                col++;
            }
            row++;
            col = 0;
        }
        board.InitialState = new BoardState(board.WhiteKings, board.WhiteQueens, board.WhiteRooks, board.WhiteBishops, board.WhiteKnights, board.WhitePawns, board.BlackKings, board.BlackQueens, board.BlackRooks, board.BlackBishops, board.BlackKnights, board.BlackPawns);
        return board;

    }

    // TODO: Wrote by Copilot, validate later
    /*
    private bool IsInCheck(PieceColor color)
    {
        var king = color == PieceColor.White ? WhiteKings : BlackKings;
        var opponentPieces = color == PieceColor.White ? BlackPieces : WhitePieces;
        var opponentMoves = GenerateMoves(opponentPieces, color == PieceColor.White ? PieceColor.Black : PieceColor.White);
        return (king & opponentMoves) != 0;
    }*/

    public Board(int windowWidth, int windowHeight)
    {
        this.windowWidth = windowWidth;
        this.windowHeight = windowHeight;
        squareWidth = windowWidth / SquareNo;
        squareHeight = windowHeight / SquareNo;
    }

    public void Run()
    {
        Log.IgnoreRaylibLogs();
        Raylib.InitWindow(windowWidth, windowHeight, "Game of Chess");
        pieceTexture = Raylib.LoadTexture("assets/Pieces.png");
        spriteWidth = pieceTexture.Width / 6;
        spriteHeight = pieceTexture.Height / 2;

        Raylib.SetTargetFPS(60);
        while (!Raylib.WindowShouldClose())
        {
            MouseController.Update();
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.WHITE);
            DrawBoard();
            DrawOverlay();
            Raylib.EndDrawing();
        }
    }

    private void SetPosition(BoardState boardState)
    {
        WhiteKings = boardState.WhiteKings;
        WhiteQueens = boardState.WhiteQueens;
        WhiteRooks = boardState.WhiteRooks;
        WhiteBishops = boardState.WhiteBishops;
        WhiteKnights = boardState.WhiteKnights;
        WhitePawns = boardState.WhitePawns;
        BlackKings = boardState.BlackKings;
        BlackQueens = boardState.BlackQueens;
        BlackRooks = boardState.BlackRooks;
        BlackBishops = boardState.BlackBishops;
        BlackKnights = boardState.BlackKnights;
        BlackPawns = boardState.BlackPawns;
    }

    private void DrawOverlay()
    {
        if (Raylib.IsKeyDown(KeyboardKey.KEY_R) && InitialState is not null)
        {
            SetPosition(InitialState.Value);
        }
        if (KeyController.LeftArrowPressed && historyIndex > 0 && history[historyIndex - 1].WhiteKings != 0)
        {
            historyIndex--;
            var state = history[historyIndex];
            WhiteKings = state.WhiteKings;
            WhiteQueens = state.WhiteQueens;
            WhiteRooks = state.WhiteRooks;
            WhiteBishops = state.WhiteBishops;
            WhiteKnights = state.WhiteKnights;
            WhitePawns = state.WhitePawns;
            BlackKings = state.BlackKings;
            BlackQueens = state.BlackQueens;
            BlackRooks = state.BlackRooks;
            BlackBishops = state.BlackBishops;
            BlackKnights = state.BlackKnights;
            BlackPawns = state.BlackPawns;
        }
        if (KeyController.RightArrowPressed && historyIndex < history.Length - 1)
        {
            if (history[historyIndex + 1].WhiteKings != 0)
            {
                Console.WriteLine("Here");
                historyIndex++;
                var state = history[historyIndex];
                WhiteKings = state.WhiteKings;
                WhiteQueens = state.WhiteQueens;
                WhiteRooks = state.WhiteRooks;
                WhiteBishops = state.WhiteBishops;
                WhiteKnights = state.WhiteKnights;
                WhitePawns = state.WhitePawns;
                BlackKings = state.BlackKings;
                BlackQueens = state.BlackQueens;
                BlackRooks = state.BlackRooks;
                BlackBishops = state.BlackBishops;
                BlackKnights = state.BlackKnights;
                BlackPawns = state.BlackPawns;
            }
            else
            {

            }
        }
        {
            // TODO: Implement redo
        }
        if (MouseController.LeftMouseButtonPressed)
        {
            var x = (int)MouseController.MousePosition.X / squareWidth;
            var y = (int)MouseController.MousePosition.Y / squareHeight;
            Console.WriteLine($"\n\n#########\n\nLeft mouse clicked at x={x}, y={y}");
            var idx = y * SquareNo + x;
            if (!TryMoveOrCapture(idx))
            {
                var pieceType = PieceTypeAt(idx);
                if (pieceType != null)
                {
                    PieceSelected = 1UL << idx;
                    PieceSelectedType = pieceType;
                    Console.WriteLine(pieceType + " at " + idx + " is selected");
                    FindMoves(pieceType.Value, idx, x, y);
                }
            }
        }
        possibleMoves.Values.ToList().ForEach(move =>
        {
            var square = new Rectangle(move.col * squareWidth, move.row * squareHeight, squareWidth, squareHeight);
            Raylib.DrawRectangleRec(square, new Color(0, 230, 80, 100));
        });
        possibleCaptures.Values.ToList().ForEach(move =>
        {
            var square = new Rectangle(move.col * squareWidth, move.row * squareHeight, squareWidth, squareHeight);
            Raylib.DrawRectangleRec(square, new Color(255, 0, 50, 100));
        });


    }

    private bool TryMoveOrCapture(int idx)
    {
        var isMoveOrCapture = false;
        if (PieceSelected != 0 && PieceSelectedType != null)
        {
            var clickedSquare = 1UL << idx;
            if (possibleMoves.ContainsKey(clickedSquare))
            {
                // TODO: Find way to detect if the move was a double push
                SaveBoardState();
                var (row, col) = possibleMoves[clickedSquare];
                var piece = PieceSelectedType.Value;
                Console.WriteLine(piece + " at " + idx + " is moved to " + row + ", " + col);
                switch (piece)
                {
                    case PieceType.WhitePawn:
                        {
                            WhitePawns &= ~PieceSelected;
                            WhitePawns |= clickedSquare;
                            break;
                        }
                    case PieceType.BlackPawn:
                        {
                            BlackPawns &= ~PieceSelected;
                            BlackPawns |= clickedSquare;
                            break;
                        }
                }
                isMoveOrCapture = true;
            }
            else if (possibleCaptures.ContainsKey(clickedSquare))
            {
                SaveBoardState();
                var (row, col) = possibleCaptures[clickedSquare];
                var piece = PieceSelectedType.Value;
                Console.WriteLine(piece + " at " + idx + " is captured at " + row + ", " + col);
                switch (piece)
                {
                    case PieceType.WhitePawn:
                        {
                            BlackPawns &= ~clickedSquare;
                            WhitePawns &= ~PieceSelected;
                            WhitePawns |= clickedSquare;
                            break;
                        }
                    case PieceType.BlackPawn:
                        {
                            WhitePawns &= ~clickedSquare;
                            BlackPawns &= ~PieceSelected;
                            BlackPawns |= clickedSquare;
                            break;
                        }
                }
                isMoveOrCapture = true;
            }
        }
        DetectPromotion();


        PieceSelected = 0;
        possibleMoves.Clear();
        possibleCaptures.Clear();
        return isMoveOrCapture;
    }

    private void DetectPromotion()
    {
        // TODO: Allow the user to select the piece to promote to
        var whitePromotion = WhitePawns & Masks.Rank8;
        if (whitePromotion != 0)
        {
            WhitePawns &= ~whitePromotion;
            WhiteQueens |= whitePromotion;
        }
        var blackPromotion = BlackPawns & Masks.Rank1;
        if (blackPromotion != 0)
        {
            BlackPawns &= ~blackPromotion;
            BlackQueens |= blackPromotion;
        }
    }

    private void FindMoves(PieceType type, int idx, int x, int y)
    {
        switch (type)
        {
            case PieceType.WhitePawn:
                {
                    FindPawnMoves(idx, x, y, PieceColor.White);
                    break;
                }
            case PieceType.BlackPawn:
                {
                    FindPawnMoves(idx, x, y, PieceColor.Black);
                    break;
                }
        }
    }

    private void FindPawnMoves(int idx, int x, int y, PieceColor color)
    {
        var isWhite = color == PieceColor.White;
        var isStartingPosition = (isWhite && y == 6) || (!isWhite && y == 1);
        var isLeftEdge = x == 0;
        var isRightEdge = x == 7;
        var pawn = (isWhite ? WhitePawns : BlackPawns) & (1UL << idx);

        var standardMove = (isWhite ? pawn >> 8 : pawn << 8) & EmptySquares;

        if (standardMove != 0)
        {
            AddToMoveCollection(standardMove, possibleMoves);
        }

        if (isStartingPosition && standardMove != 0)
        {
            var doublePush = (isWhite ? pawn >> 16 : pawn << 16) & EmptySquares;
            if (doublePush != 0)
            {
                AddToMoveCollection(doublePush, possibleMoves);
            }
        }

        PrintBitBoard(pawn, "Current pawn");
        if (historyIndex > 0 && y == (isWhite ? 3 : 4))
        {
            // Check if en passant is possible
            var lastState = history[historyIndex - 1];
            var lastMove = isWhite ? BlackPawns & ~lastState.BlackPawns : WhitePawns & ~lastState.WhitePawns;
            PrintBitBoard(lastMove, "Last move");
            PrintBitBoard(lastState.WhitePawns, "White pawns in former state");
            PrintBitBoard(lastState.BlackPawns, "Black pawns in former state");

            var lastMovePosition = BitOperations.TrailingZeroCount(lastMove);
            var lastMoveX = lastMovePosition % SquareNo;
            var lastMoveY = lastMovePosition / SquareNo;

            if (lastMoveY == (isWhite ? 3 : 4) && (lastMoveX == x - 1 || lastMoveX == x + 1))
            {
                Console.WriteLine("En passant is possible");
                var enPassantPosition = isWhite ? lastMovePosition - 8 : lastMovePosition + 8;
                var enPassant = 1UL << enPassantPosition;
                if (enPassant != 0)
                {
                    AddToMoveCollection(enPassant, possibleCaptures);
                }
            }
        }

        var opponentPieces = isWhite ? BlackPieces : WhitePieces;
        if (!isRightEdge)
        {
            var captureRight = (isWhite ? pawn >> 7 : pawn << 7) & opponentPieces;
            if (captureRight != 0)
                AddToMoveCollection(captureRight, possibleCaptures);
        }
        if (!isLeftEdge)
        {
            var captureLeft = (isWhite ? pawn >> 9 : pawn << 9) & opponentPieces;
            if (captureLeft != 0)
                AddToMoveCollection(captureLeft, possibleCaptures);

        }
    }

    private static void AddToMoveCollection(ulong move, Dictionary<ulong, (int, int)> collection)
    {
        int position = BitOperations.TrailingZeroCount(move);
        int row = position / SquareNo;
        int col = position % SquareNo;
        collection[move] = (row, col);
    }

    private float MaterialEvaluation(ulong bitBoard)
    {
        var score = 0f;
        var idx = 0;
        while (bitBoard != 0)
        {
            if ((bitBoard & 1) != 0)
            {
                score += PieceValue(idx);
            }
            bitBoard >>= 1;
            idx++;
        }
        return score;
    }

    public float Evaluation(ulong bitBoard)
    {
        var score = MaterialEvaluation(bitBoard);
        // TODO: Take positional evaluation into account
        return score;
    }

    private float PieceValue(int index)
    {
        var row = index / SquareNo;
        var col = index % SquareNo;
        var pieceType = PieceTypeAt(index);
        var value = 0f;
        switch (pieceType)
        {
            case PieceType.WhiteKing:
                value = 200f;
                break;
            case PieceType.WhiteQueen:
                value = 9f;
                break;
            case PieceType.WhiteRook:
                value = 5f;
                break;
            case PieceType.WhiteBishop:
                value = 3.25f;
                break;
            case PieceType.WhiteKnight:
                value = 3f;
                break;
            case PieceType.WhitePawn:
                value = 1f;
                break;
            case PieceType.BlackKing:
                value = -200f;
                break;
            case PieceType.BlackQueen:
                value = -9f;
                break;
            case PieceType.BlackRook:
                value = -5f;
                break;
            case PieceType.BlackBishop:
                value = -3.25f;
                break;
            case PieceType.BlackKnight:
                value = -3f;
                break;
            case PieceType.BlackPawn:
                value = -1f;
                break;
        }
        if (pieceType == PieceType.WhitePawn)
        {
            value += 0.1f * (7 - row);
        }
        if (pieceType == PieceType.BlackPawn)
        {
            value -= 0.1f * row;
        }
        return value;
    }

    private PieceType? PieceTypeAt(int index)
    {
        if (GetBit(WhiteKings, index) != 0) return PieceType.WhiteKing;
        if (GetBit(WhiteQueens, index) != 0) return PieceType.WhiteQueen;
        if (GetBit(WhiteRooks, index) != 0) return PieceType.WhiteRook;
        if (GetBit(WhiteBishops, index) != 0) return PieceType.WhiteBishop;
        if (GetBit(WhiteKnights, index) != 0) return PieceType.WhiteKnight;
        if (GetBit(WhitePawns, index) != 0) return PieceType.WhitePawn;
        if (GetBit(BlackKings, index) != 0) return PieceType.BlackKing;
        if (GetBit(BlackQueens, index) != 0) return PieceType.BlackQueen;
        if (GetBit(BlackRooks, index) != 0) return PieceType.BlackRook;
        if (GetBit(BlackBishops, index) != 0) return PieceType.BlackBishop;
        if (GetBit(BlackKnights, index) != 0) return PieceType.BlackKnight;
        if (GetBit(BlackPawns, index) != 0) return PieceType.BlackPawn;
        return null;
    }


    private void DrawBoard()
    {

        for (int y = 0; y < SquareNo; y++)
            for (int x = 0; x < SquareNo; x++)
            {
                var idx = y * SquareNo + x;
                var square = new Rectangle(x * squareWidth, y * squareHeight, squareWidth, squareHeight);
                Raylib.DrawRectangleRec(square, (x + y) % 2 == 0 ? boardBaseColor.BrightenUp(0.8f) : boardBaseColor);
                if (GetBit(WhiteKings, idx) != 0) DrawPiece(PieceType.WhiteKing, x, y);
                if (GetBit(WhiteQueens, idx) != 0) DrawPiece(PieceType.WhiteQueen, x, y);
                if (GetBit(WhiteRooks, idx) != 0) DrawPiece(PieceType.WhiteRook, x, y);
                if (GetBit(WhiteBishops, idx) != 0) DrawPiece(PieceType.WhiteBishop, x, y);
                if (GetBit(WhiteKnights, idx) != 0) DrawPiece(PieceType.WhiteKnight, x, y);
                if (GetBit(WhitePawns, idx) != 0) DrawPiece(PieceType.WhitePawn, x, y);
                if (GetBit(BlackKings, idx) != 0) DrawPiece(PieceType.BlackKing, x, y);
                if (GetBit(BlackQueens, idx) != 0) DrawPiece(PieceType.BlackQueen, x, y);
                if (GetBit(BlackRooks, idx) != 0) DrawPiece(PieceType.BlackRook, x, y);
                if (GetBit(BlackBishops, idx) != 0) DrawPiece(PieceType.BlackBishop, x, y);
                if (GetBit(BlackKnights, idx) != 0) DrawPiece(PieceType.BlackKnight, x, y);
                if (GetBit(BlackPawns, idx) != 0) DrawPiece(PieceType.BlackPawn, x, y);
            }


    }

    private void DrawPiece(PieceType pieceType, int x, int y)
    {
        var pieceX = (int)pieceType % 6;
        var pieceY = (int)pieceType / 6;
        var source = new Rectangle(pieceX * spriteWidth, pieceY * spriteHeight, spriteWidth, spriteHeight);
        var dest = new Rectangle(x * squareWidth, y * squareHeight, squareWidth, squareHeight);
        Raylib.DrawTexturePro(pieceTexture, source, dest, Vector2.Zero, 0f, Color.WHITE);
    }

}