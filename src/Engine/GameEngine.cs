using Raylib_cs;
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
    private readonly Opponent opponent = Opponent.ComputerIsBlack;
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

        controller = new GameController();
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
        if (opponent == Opponent.None || controller.Status != GameController.GameStatus.Ongoing) return;
        if (opponent == Opponent.ComputerIsWhite && !controller.WhiteToPlay)
            return;
        if (opponent == Opponent.ComputerIsBlack && controller.WhiteToPlay)
            return;
        var moves = controller.GetValidMoves();
        var randomMove = moves[random.Next(moves.Count)];
        controller.MakeMove(randomMove);
    }

    private void HandleInput()
    {
        if (InputHandler.IsKeyPressed(KeyboardKey.KEY_F))
        {
            useStandardOrientation = !useStandardOrientation;
            renderer.UpdateOrientation(useStandardOrientation);
        }
        if (!InputHandler.IsLeftMouseButtonPressed) return;

        var mousePos = inputHandler.GetMouseGridPosition();
        if (!InputHandler.IsMouseOnBoard(mousePos)) return;

        int squareIndex = BoardUtility.IndexUnderMouse(mousePos, useStandardOrientation);

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

        if (controller.SelectedPiece.HasValue)
        {
            var moves = controller.GetValidMovesForSelected();
            renderer.HighlightSquares(moves.ToBitboard(), Color.GREEN);
        }

        var lastEntry = controller.LastEntry();
        if (lastEntry.HasValue)
        {
            var lastMove = lastEntry.Value.Move;
            renderer.HighlightSquares(lastMove.OriginBit | lastMove.TargetBit, Color.BLUE);
        }

        if (controller.KingIsUnderAttack)
        {
            var theKing = controller.BoardState.GetPieceBoard(PieceType.King);
            renderer.HighlightSquares(theKing, Color.RED);
        }

        renderer.DrawPieces(controller.BoardState);
        Raylib.EndDrawing();
    }
}