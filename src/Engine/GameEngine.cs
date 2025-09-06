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
    private PieceSelection? _selectedPiece;

    public GameEngine()
    {
        var windowHeight = RaylibUtility.GetWindowHeightDynamically();
        _sideLength = windowHeight / Constants.SquareCount;

        _windowSize = (windowHeight, windowHeight);

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
        }
    }



    private void HandleInput()
    {
        var mouseGridPos = _inputHandler.GetMouseGridPosition();
        var isMouseOnBoard = InputHandler.IsMouseOnBoard(mouseGridPos);
        if (InputHandler.IsLeftMouseButtonPressed && isMouseOnBoard)
        {
            var (index, bit) = BoardUtility.IndexAndBitUnderMouse(mouseGridPos);

            if (_selectedPiece.HasValue && Board.IsValidMove(_selectedPiece.Value, bit))
            {
                _board.MakeMove(_selectedPiece.Value, bit);
                _selectedPiece = null;
            }
            else
            {
                _selectedPiece = _board.TrySelectPiece(index);
            }
        }
    }

    private void Render()
    {

        var bgColor = new Color(4, 15, 15, 1);
        Raylib.BeginDrawing();
        Raylib.ClearBackground(bgColor);
        _renderer.DrawBoard();
        _renderer.DrawPieces(_board);

        if (_selectedPiece.HasValue)
        {
            _renderer.HighlightSquares(_selectedPiece.Value.LegalMoves, Color.GREEN);
        }

        Raylib.EndDrawing();
    }


}