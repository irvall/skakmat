using Raylib_cs;
using skakmat.Extensions;
using skakmat.Game;
using skakmat.Helpers;
using skakmat.Rendering;

namespace skakmat.Engine;

internal class GameEngine
{
    private readonly GameController gameController;
    private readonly InputHandler inputHandler;
    private const Opponent OpposingPlayer = Opponent.ComputerIsWhite;
    private readonly Random random = new();
    private readonly BoardRenderer renderer;
    private bool useStandardOrientation;

    public GameEngine()
    {
        var windowHeight = RaylibHelper.GetWindowHeightDynamically();
        var sideLength = windowHeight / Constants.SquareCount;

        useStandardOrientation = OpposingPlayer != Opponent.ComputerIsWhite;
        renderer = new BoardRenderer(windowHeight, sideLength, useStandardOrientation);
        inputHandler = new InputHandler(sideLength);

        gameController = new GameController();
        var soundHandler1 = new GameSoundHandler();

        gameController.GameEventOccurred += soundHandler1.HandleGameEvent;
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
        if (OpposingPlayer == Opponent.None || gameController.Status != GameStatus.Ongoing) return;
        if (OpposingPlayer == Opponent.ComputerIsWhite && !gameController.WhiteToPlay)
            return;
        var move = FindComputerMove();
        gameController.MakeMove(move);
    }

    private Move FindComputerMove()
    {
        var moves = gameController.GetValidMoves();
        return moves[random.Next(moves.Count)];
    }

    private void HandleInput()
    {
        if (InputHandler.IsKeyPressed(KeyboardKey.F))
        {
            useStandardOrientation = !useStandardOrientation;
            renderer.UpdateOrientation(useStandardOrientation);
        }

        if (InputHandler.IsLeftArrowPressed) gameController.StepBack();
        if (InputHandler.IsRightArrowPressed) gameController.StepForward();

        if (!InputHandler.IsLeftMouseButtonPressed || !gameController.AtMostRecentPosition()) return;

        var mousePos = inputHandler.GetMouseGridPosition();
        if (!InputHandler.IsMouseOnBoard(mousePos)) return;

        var squareIndex = BoardHelper.IndexUnderMouse(mousePos, useStandardOrientation);

        if (gameController.SelectedPiece.HasValue)
        {
            var selection = gameController.SelectedPiece.Value;
            var originBit = 1UL << selection.SquareIndex;
            var targetBit = 1UL << squareIndex;
            var moveAttempt = new Move(selection.PieceIndex, originBit, targetBit);

            if (gameController.IsValidMove(moveAttempt)) gameController.MakeMove(moveAttempt);

            gameController.ClearSelection();
        }

        gameController.SelectPiece(squareIndex);
    }

    private void Render()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(4, 15, 15, 1));
        renderer.DrawBoard();

        var position = gameController.CurrentPosition;
        if (gameController.SelectedPiece.HasValue)
        {
            var moves = gameController.GetValidMovesForSelected();
            renderer.HighlightSquares(moves.ToBitboard(), Color.Green);
        }

        var lastMove = position.LastMovePlayed;
        if (lastMove is not null)
            renderer.HighlightSquares(lastMove.OriginBit | lastMove.TargetBit, Color.Blue);

        if (gameController.KingIsUnderAttack)
        {
            var theKing = position.GetPieceBoard(PieceType.King);
            renderer.HighlightSquares(theKing, Color.Red);
        }

        renderer.DrawPieces(position);
        Raylib.EndDrawing();
    }

    private enum Opponent
    {
        None,
        ComputerIsWhite,
        ComputerIsBlack
    }
}