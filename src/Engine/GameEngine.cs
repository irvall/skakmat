using Raylib_cs;
using skakmat.Chess;
using skakmat.Game;
using skakmat.Rendering;
using skakmat.Utilities;
using static skakmat.Engine.GameController;

namespace skakmat.Engine;

class GameEngine

{
    private readonly Board _board;
    private readonly MoveGenerator _moveGen;
    private readonly BoardRenderer _renderer;
    private readonly InputHandler _inputHandler;
    private readonly GameController _gameController;

    private readonly (int width, int height) _windowSize;
    private readonly int _sideLength;
    private readonly Random _random;
    private readonly bool showControlledSquares = false;
    private readonly bool playAgainstComputer;
    private BoardState _state;
    private PieceSelection? _selectedPiece;

    internal GameEngine(bool playAgainstComputer)
    {
        var windowHeight = RaylibUtility.GetWindowHeightDynamically();
        _sideLength = windowHeight / Constants.SquareCount;
        this.playAgainstComputer = playAgainstComputer;

        _windowSize = (windowHeight, windowHeight);
        _random = new Random();

        _board = new Board();
        _state = _board.GetBoardState();
        _moveGen = new MoveGenerator(new MoveTables(), _board);
        _gameController = new GameController(_board, _moveGen);
        _renderer = new BoardRenderer(windowHeight, _sideLength);
        _inputHandler = new InputHandler(_sideLength);
    }

    internal void Run()
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

    private bool shouldRegenerateMoves;

    private void GameLoop()
    {
        var validMoves = _moveGen.GenerateMoves();
        while (!Raylib.WindowShouldClose())
        {
            if (shouldRegenerateMoves)
            {
                _state = _board.GetBoardState();
                validMoves = _moveGen.GenerateMoves();
                shouldRegenerateMoves = false;
                _gameController.UpdateGameStatus();
                Thread.Sleep(50);
            }
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
        if (playAgainstComputer && !_state.WhiteToPlay)
        {
            // TODO: Allow AI to play as white
            var computerMove = GetComputerMove(validMoves);
            _board.ExecuteMove(computerMove);
            shouldRegenerateMoves = true;
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
            var targetMove = new Move(_selectedPiece.Value.PieceIndex, originBit, bit);

            if (validMoves.Any(m => m.Equals(targetMove)))
            {
                var actualMove = validMoves.First(m => m.Equals(targetMove));
                _board.ExecuteMove(actualMove);
                shouldRegenerateMoves = true;
                _selectedPiece = null;
                return;
            }
        }

        _selectedPiece = _gameController.TrySelectPiece(index);
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

        if (_gameController.MovesPlayed > 0)
        {
            var lastEntry = _gameController.LastEntry();
            _renderer.HighlightSquares(lastEntry.Move.OriginBit, Color.BLUE);
            _renderer.HighlightSquares(lastEntry.Move.TargetBit, Color.BLUE);
        }

        if (showControlledSquares)
        {
            var controlled = _moveGen.SquaresUnderControl(_state.WhiteToPlay);
            _renderer.HighlightSquares(controlled, Color.RED);
        }

        _renderer.DrawPieces(_board);

        if (_gameController.Status == GameStatus.WhiteWon)
        {
            _renderer.HighlightSquares(~0UL, Color.SKYBLUE);
            _renderer.DrawBigMessage("YOUR WINNER");
        }

        if (_gameController.Status == GameStatus.BlackWon)
        {
            _renderer.HighlightSquares(~0UL, Color.SKYBLUE);
            _renderer.DrawBigMessage("STALE MAT");
        }

        if (_gameController.Status == GameStatus.Stalemate)
        {
            _renderer.HighlightSquares(~0UL, Color.SKYBLUE);
            _renderer.DrawBigMessage("YOUR LOOSE");
        }


        Raylib.EndDrawing();
    }

}