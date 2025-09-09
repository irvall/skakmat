using Raylib_cs;
using skakmat.Game;
using skakmat.Rendering;
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
    private readonly bool showControlledSquares = false;
    private readonly bool playAgainstComputer;

    enum GameStatus
    {
        GameOn,
        WhiteWon,
        BlackWon,
        Stalemate
    }

    private GameStatus status = GameStatus.GameOn;

    public GameEngine(bool playAgainstComputer)
    {
        var windowHeight = RaylibUtility.GetWindowHeightDynamically();
        _sideLength = windowHeight / Constants.SquareCount;
        this.playAgainstComputer = playAgainstComputer;

        _windowSize = (windowHeight, windowHeight);
        _random = new Random();

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
            var validMoves = _board.GenerateMoves();
            HandleInput(validMoves);
            Render();
        }
    }

    private Move GetComputerMove(List<Move> validMoves)
    {
        var randomMove = validMoves[_random.Next(validMoves.Count)];
        return randomMove;
    }

    private void HandleInput(List<Move> validMoves)
    {
        if (validMoves.Count == 0)
        {
            if (!_board.IsKingUnderAttack())
            {
                status = GameStatus.Stalemate;
            }
            else
            {
                status = _board.WhiteToPlay ? GameStatus.BlackWon : GameStatus.WhiteWon;
            }
            return;
        }

        if (playAgainstComputer && !_board.WhiteToPlay)
        {
            // TODO: Allow AI to play as white
            var computerMove = GetComputerMove(validMoves);
            _board.MakeMove(computerMove);
            return;
        }

        if (!InputHandler.IsLeftMouseButtonPressed)
            return;

        var mouseGridPos = _inputHandler.GetMouseGridPosition();
        if (!InputHandler.IsMouseOnBoard(mouseGridPos))
            return;

        var (index, bit) = BoardUtility.IndexAndBitUnderMouse(mouseGridPos);
        if (_selectedPiece.HasValue)
        {
            var originBit = 1UL << _selectedPiece.Value.SquareIndex;
            var targetMove = new Move(_selectedPiece.Value.PieceType, originBit, bit);

            if (validMoves.Any(m => m.Equals(targetMove)))
            {
                var actualMove = validMoves.First(m => m.Equals(targetMove));
                _board.MakeMove(actualMove);
                _selectedPiece = null;
                return;
            }
        }

        _selectedPiece = _board.TrySelectPiece(index);
    }

    private void Render()
    {
        var bgColor = new Color(4, 15, 15, 1);
        Raylib.BeginDrawing();
        Raylib.ClearBackground(bgColor);
        _renderer.DrawBoard();

        if (_selectedPiece.HasValue)
        {
            var movesBitboard = _selectedPiece.Value.ValidMoves.ToBitboard();
            _renderer.HighlightSquares(movesBitboard, Color.GREEN);

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

        if (status == GameStatus.Stalemate)
        {
            _renderer.HighlightSquares(~0UL, Color.SKYBLUE);
            _renderer.DrawBigMessage("STALE MAT");
        }

        if (status == GameStatus.BlackWon)
        {
            _renderer.HighlightSquares(~0UL, Color.SKYBLUE);
            _renderer.DrawBigMessage("YOUR LOOSE");
        }


        Raylib.EndDrawing();
    }

}