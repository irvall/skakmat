using Raylib_cs;
using skakmat.Game;
using skakmat.Rendering;
using skakmat.Utilities;

namespace skakmat.Engine;

internal class GameEngine
{
    private readonly BoardRenderer renderer;
    private readonly InputHandler inputHandler;
    private readonly GameSoundHandler soundHandler;
    private readonly Random random = new();
    private readonly int sideLength;
    private readonly GameController gameController;
    private readonly Opponent opponent = Opponent.ComputerIsWhite;
    private bool useStandardOrientation;

    enum Opponent
    {
        None,
        ComputerIsWhite,
        ComputerIsBlack
    }

    public GameEngine()
    {
        var windowHeight = RaylibUtility.GetWindowHeightDynamically();
        sideLength = windowHeight / Constants.SquareCount;

        useStandardOrientation = opponent != Opponent.ComputerIsWhite;
        renderer = new BoardRenderer(windowHeight, sideLength, useStandardOrientation);
        inputHandler = new InputHandler(sideLength);

        gameController = new GameController();
        soundHandler = new GameSoundHandler();

        gameController.GameEventOccurred += soundHandler.HandleGameEvent;
    }

    internal void Run()
    {
        renderer.Initialize();
        GameLoop();
        Raylib.CloseWindow();
    }


    private void GameLoop()
    {
        while (!Raylib.WindowShouldClose())
        {
            HandleComputerMove();
            HandleInput();
            Render();
        }
    }

    private void HandleComputerMove()
    {
        if (opponent == Opponent.None || gameController.Status != GameStatus.Ongoing) return;
        if (opponent == Opponent.ComputerIsWhite && !gameController.WhiteToPlay)
            return;
        if (opponent == Opponent.ComputerIsBlack && gameController.WhiteToPlay)
            return;
        var moves = gameController.GetValidMoves();
        var randomMove = moves[random.Next(moves.Count)];
        Thread.Sleep(random.Next(250, 1000));
        gameController.MakeMove(randomMove);
    }

    private void HandleInput()
    {
        if (InputHandler.IsKeyPressed(KeyboardKey.KEY_F))
        {
            useStandardOrientation = !useStandardOrientation;
            renderer.UpdateOrientation(useStandardOrientation);
        }
        if (InputHandler.IsLeftArrowPressed)
        {
            gameController.StepBack();
        }
        if (InputHandler.IsRightArrowPressed)
        {
            gameController.StepForward();
        }

        if (!InputHandler.IsLeftMouseButtonPressed || !gameController.AtMostRecentState()) return;

        var mousePos = inputHandler.GetMouseGridPosition();
        if (!InputHandler.IsMouseOnBoard(mousePos)) return;

        int squareIndex = BoardUtility.IndexUnderMouse(mousePos, useStandardOrientation);

        if (gameController.SelectedPiece.HasValue)
        {
            var selection = gameController.SelectedPiece.Value;
            var originBit = 1UL << selection.SquareIndex;
            var targetBit = 1UL << squareIndex;
            var moveAttempt = new Move(selection.PieceIndex, originBit, targetBit);

            if (gameController.IsValidMove(moveAttempt))
            {
                gameController.MakeMove(moveAttempt);
            }

            gameController.ClearSelection();
        }
        gameController.SelectPiece(squareIndex);
    }

    private void Render()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(4, 15, 15, 1));
        renderer.DrawBoard();
        var boardState = gameController.GetCurrentState();

        if (gameController.SelectedPiece.HasValue)
        {
            var moves = gameController.GetValidMovesForSelected();
            renderer.HighlightSquares(moves.ToBitboard(), Color.GREEN);
        }

        var lastMove = boardState.LastMovePlayed;
        if (lastMove is not null)
            renderer.HighlightSquares(lastMove.OriginBit | lastMove.TargetBit, Color.BLUE);

        if (gameController.KingIsUnderAttack)
        {
            var theKing = gameController.BoardState.GetPieceBoard(PieceType.King);
            renderer.HighlightSquares(theKing, Color.RED);
        }

        renderer.DrawPieces(boardState);
        Raylib.EndDrawing();
    }
}