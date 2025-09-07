using Raylib_cs;
using skakmat.Game;
using skakmat.Rendering;
using skakmat.Utilites;
using skakmat.Utilities;

namespace skakmat.Engine;

class GameEngine

{
    private readonly Board _board;
    private readonly BoardRenderer _renderer;
    private readonly InputHandler _inputHandler;

    private readonly (int width, int height) _windowSize;
    private readonly int _sideLength;
    private readonly Random _random;
    private PieceSelection? _selectedPiece;
    private bool _debug = true;
    private bool _dumpBoard = true;

    public GameEngine()
    {
        var windowHeight = RaylibUtility.GetWindowHeightDynamically();
        _sideLength = windowHeight / Constants.SquareCount;

        _windowSize = (windowHeight, windowHeight);
        _random = new Random(256);

        _board = new Board();
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
            if (_debug && _dumpBoard)
            {
                _board.DumpBoardData();
                _dumpBoard = false;
            }
        }
    }

    private void HandleInput()
    {
        if (!_board.WhiteToPlay)
        {
            var validMoves = _board.GenerateMoves();
            var randomMove = validMoves[_random.Next(validMoves.Count)];
            _board.MakeMove(randomMove);
            Thread.Sleep(_random.Next(200, 800));
            _dumpBoard = true;
        }
        var mouseGridPos = _inputHandler.GetMouseGridPosition();
        var isMouseOnBoard = InputHandler.IsMouseOnBoard(mouseGridPos);
        if (InputHandler.IsLeftMouseButtonPressed && isMouseOnBoard)
        {
            var (index, bit) = BoardUtility.IndexAndBitUnderMouse(mouseGridPos);

            if (_selectedPiece.HasValue && Board.IsValidMove(_selectedPiece.Value, bit))
            {
                var originBit = 1UL << _selectedPiece.Value.SquareIndex;
                var move = new Move(_selectedPiece.Value.PieceType, originBit, bit);
                _board.MakeMove(move);
                _selectedPiece = null;
            }
            else
            {
                _selectedPiece = _board.TrySelectPiece(index);
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

        _renderer.DrawPieces(_board);
        Raylib.EndDrawing();
    }

}