using Raylib_cs;
using skakmat.Game;
using skakmat.Rendering;
using skakmat.Utilites;
using skakmat.Utilities;

namespace skakmat.Engine;

class GameEngine

{
    private readonly bool _debug = true;
    private readonly Board _board;
    private readonly BoardRenderer _renderer;
    private readonly InputHandler _inputHandler;

    private readonly (int width, int height) _windowSize;
    private readonly int _sideLength;
    private readonly Random _random;
    private PieceSelection? _selectedPiece;
    private readonly bool showControlledSquares = false;
    private readonly bool playAgainstComputer = true;

    enum GameStatus
    {
        GameOn,
        WhiteWon,
        BlackWon
    }

    private GameStatus status = GameStatus.GameOn;

    public GameEngine()
    {
        var windowHeight = RaylibUtility.GetWindowHeightDynamically();
        _sideLength = windowHeight / Constants.SquareCount;

        _windowSize = (windowHeight, windowHeight);
        _random = new Random(256);

        _board = new Board(_debug);
        _renderer = new BoardRenderer(windowHeight, _sideLength);
        _inputHandler = new InputHandler(_sideLength);
    }

    public void Run()
    {
        InitializeWindow();
        _renderer.Initialize();
        GameLoop();
        Raylib.CloseWindow();
    }

    private void InitializeWindow()
    {
        var windowSize = _windowSize.height + _sideLength;
        Raylib.InitWindow(windowSize, windowSize, "Skakmat");
    }

    private void GameLoop()
    {
        while (!Raylib.WindowShouldClose())
        {
            HandleInput();
            Render();
        }
    }

    private Move GetComputerMove(List<Move> validMoves)
    {
        var randomMove = validMoves[_random.Next(validMoves.Count)];
        // TODO: Until proper AI, simulate "thinking"
        Thread.Sleep(_random.Next(200, 800));
        return randomMove;
    }

    private void HandleInput()
    {
        var validMoves = _board.GenerateMoves();
        if (validMoves.Count == 0)
        {
            status = _board.WhiteToPlay ? GameStatus.BlackWon : GameStatus.WhiteWon;
            return;
        }
        if (playAgainstComputer && !_board.WhiteToPlay)
        {
            // TODO: Allow AI to play as white
            var computerMove = GetComputerMove(validMoves);
            _board.MakeMove(computerMove);
        }
        if (InputHandler.IsLeftMouseButtonPressed)
        {
            var mouseGridPos = _inputHandler.GetMouseGridPosition();
            var isMouseOnBoard = InputHandler.IsMouseOnBoard(mouseGridPos);
            if (isMouseOnBoard)
            {
                var (index, bit) = BoardUtility.IndexAndBitUnderMouse(mouseGridPos);

                if (_selectedPiece.HasValue && Board.IsPseudoValidMove(_selectedPiece.Value, bit))
                {
                    var originBit = 1UL << _selectedPiece.Value.SquareIndex;
                    var move = new Move(_selectedPiece.Value.PieceType, originBit, bit);

                    if (validMoves.ToHashSet().Contains(move))
                    {
                        _board.MakeMove(move);
                        _selectedPiece = null;
                    }
                }
                else
                {
                    _selectedPiece = _board.TrySelectPiece(index);
                }
            }
        }
        if (InputHandler.IsLeftArrowPressed)
            _board.UndoMove();

    }

    private void Render()
    {
        var bgColor = new Color(4, 15, 15, 1);
        Raylib.BeginDrawing();
        Raylib.ClearBackground(bgColor);
        _renderer.DrawBoard();

        if (_selectedPiece.HasValue)
        {
            _renderer.HighlightSquares(_selectedPiece.Value.LegalMoves, Color.GREEN);
        }


        if (_board.MovesPlayed > 0)
        {
            var lastMove = _board.GetMoveAt(_board.MovesPlayed - 1);
            _renderer.HighlightSquares(lastMove.OriginBit, Color.BLUE);
            _renderer.HighlightSquares(lastMove.TargetBit, Color.BLUE);
        }

        if (showControlledSquares)
        {
            var controlled = _board.ControlledSquares();
            _renderer.HighlightSquares(controlled, Color.RED);
        }

        _renderer.DrawPieces(_board);

        if (status == GameStatus.WhiteWon)
        {
            _renderer.HighlightSquares(~0UL, Color.SKYBLUE);
            _renderer.DrawBigMessage("YOUR WINNER");
        }

        if (status == GameStatus.BlackWon)
        {
            _renderer.HighlightSquares(~0UL, Color.SKYBLUE);
            _renderer.DrawBigMessage("YOUR LOOSE");
        }


        Raylib.EndDrawing();
    }

}