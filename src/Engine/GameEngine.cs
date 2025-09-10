using Raylib_cs;
using skakmat.Chess;
using skakmat.Game;
using skakmat.Rendering;
using skakmat.Utilities;

namespace skakmat.Engine;

internal class GameEngine
{
    private readonly BoardRenderer renderer;
    private readonly InputHandler inputHandler;
    private readonly Random random = new();
    private readonly int sideLength;

    private readonly GameController controller;

    public GameEngine()
    {
        var windowHeight = RaylibUtility.GetWindowHeightDynamically();
        sideLength = windowHeight / Constants.SquareCount;

        renderer = new BoardRenderer(windowHeight, sideLength);
        inputHandler = new InputHandler(sideLength);

        controller = new GameController();
    }

    internal void Run()
    {
        InitializeWindow();
        renderer.Initialize();
        GameLoop();
        Raylib.CloseWindow();
    }

    private static void InitializeWindow()
    {
        var windowHeight = RaylibUtility.GetWindowHeightDynamically();
        var sideLength = windowHeight / 8;
        var windowSize = windowHeight + sideLength;
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
        if (!InputHandler.IsLeftMouseButtonPressed) return;

        var mousePos = inputHandler.GetMouseGridPosition();
        if (!InputHandler.IsMouseOnBoard(mousePos)) return;

        int squareIndex = BoardUtility.IndexUnderMouse(mousePos);

        if (controller.SelectedPiece.HasValue)
        {
            var selection = controller.SelectedPiece.Value;
            var originBit = 1UL << selection.SquareIndex;
            var targetBit = 1UL << squareIndex;
            var moveAttempt = new Move(selection.PieceIndex, originBit, targetBit);

            if (controller.IsValidMove(moveAttempt))
            {
                controller.MakeMove(moveAttempt);
            }

            controller.ClearSelection();
        }
        else
        {
            controller.SelectPiece(squareIndex);
        }
    }

    private void Render()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(4, 15, 15, 1));

        renderer.DrawBoard();
        renderer.DrawPieces(controller.GetBoardState());

        if (controller.SelectedPiece.HasValue)
        {
            var moves = controller.GetValidMovesForSelected();
            renderer.HighlightSquares(moves.ToBitboard(), Color.GREEN);
        }

        Raylib.EndDrawing();
    }
}