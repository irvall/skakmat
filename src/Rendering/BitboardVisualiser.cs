using System.Numerics;
using Raylib_cs;
using skakmat.Game;
using skakmat.Helpers;
using Color = Raylib_cs.Color;

namespace skakmat.Rendering;

internal class BitboardVisualiser
{
    private readonly List<HighlightSquare> boundingBoxes;
    private readonly int halfSideLength;
    private readonly int sideLength;
    private readonly Vector2 upperBounds;
    private readonly (int width, int height) windowSize;
    private Color boundingBoxColor;

    private bool isDragging;
    private Vector2? rectBeginPos;
    private Vector2? rectEndPos;


    internal BitboardVisualiser()
    {
        RaylibHelper.IgnoreLogs();
        Raylib.InitWindow(0, 0, "Temporary 0x0 window to get screen size");
        var windowHeight = (int)(Raylib.GetScreenHeight() / 1.5);
        Raylib.CloseWindow();
        sideLength = windowHeight / Constants.SquareCount;
        halfSideLength = sideLength / 2;
        windowSize.width = windowHeight + sideLength;
        windowSize.height = windowHeight + sideLength;
        boundingBoxes = [];
        upperBounds = new Vector2(Constants.SquareCount - 1);
        rectBeginPos = null;
        rectEndPos = null;
        boundingBoxColor = Palette.GetNextColor();
    }

    internal void Run()
    {
        DrawWindow();
    }


    private static ulong BoundingBoxesToBitboard(List<HighlightSquare> squares)
    {
        return squares.Aggregate(0UL,
            (bitboard, nextSquare) => bitboard | BoundingBoxToBitboard(nextSquare));
    }
    private static ulong BoundingBoxToBitboard(HighlightSquare bb)
    {
        var bitboard = 0UL;
        var isXGreater = bb.EndPosition.X > bb.StartPosition.X;
        var isYGreater = bb.EndPosition.Y > bb.StartPosition.Y;
        for (var x = (int)bb.StartPosition.X;
             isXGreater ? x <= bb.EndPosition.X : x >= bb.EndPosition.X;
             x += isXGreater ? 1 : -1)
            for (var y = (int)bb.StartPosition.Y;
                 isYGreater ? y <= bb.EndPosition.Y : y >= bb.EndPosition.Y;
                 y += isYGreater ? 1 : -1)
                bitboard |= 1UL << GridIndex(x, y);

        return bitboard;
    }

    private Vector2 ScreenToGrid(int screenX, int screenY)
    {
        var gridX = (screenX - halfSideLength) / sideLength;
        var gridY = (screenY - halfSideLength) / sideLength;
        return new Vector2(gridX, gridY);
    }

    private void DrawWindow()
    {
        Raylib.InitWindow(windowSize.width, windowSize.height, "Bitboard Helper");
        var bgColor = new Color(4, 15, 15, 1);
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(bgColor);
            DrawBoard();
            HandleInteractions();
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }


    private static int GridIndex(int x, int y)
    {
        return y * Constants.SquareCount + x;
    }

    private void DrawBoard()
    {
        for (var i = 0; i < Constants.SquareCount; i++)
            for (var j = 0; j < Constants.SquareCount; j++)
            {
                if (j == 0)
                {
                    var posX = halfSideLength / 3;
                    var posY = (int)(sideLength * .85);
                    Raylib.DrawText(8 - i + "", posX, i * sideLength + posY, halfSideLength, Color.WHITE);
                }

                var draw = (i + j) % 2 != 0;
                DrawTile(j, i, draw ? Color.BROWN : Color.RAYWHITE);
            }

        for (var c = 'A'; c <= 'H'; c++)
        {
            var posX = (int)(sideLength * .85);
            var posY = windowSize.height - halfSideLength;
            Raylib.DrawText(c + "", (c - 'A') * sideLength + posX, posY, halfSideLength, Color.WHITE);
        }


        boundingBoxes.ForEach(bb => DrawRect(bb.StartPosition, bb.EndPosition, bb.Color));
    }

    private static string GetAreaAsVariable(HighlightSquare bb)
    {
        var bitboard = BoundingBoxToBitboard(bb);
        var lowerBound = Math.Min(bb.StartPosition.X, bb.EndPosition.X);
        var upperBound = Math.Max(bb.StartPosition.X, bb.EndPosition.X);
        var leftBound = Math.Max(bb.StartPosition.Y, bb.EndPosition.Y);
        var rightBound = Math.Min(bb.StartPosition.Y, bb.EndPosition.Y);
        var lowerVector = new Vector2(lowerBound, leftBound);
        var upperVector = new Vector2(upperBound, rightBound);
        var startSquare = VectorToSquare(lowerVector);
        var endSquare = VectorToSquare(upperVector);
        var bitboardAsHex = Convert.ToString((long)bitboard, 16);
        return $"internal static ulong {startSquare}{endSquare} = 0x{bitboardAsHex};";
    }

    private static string VectorToSquare(Vector2 vector)
    {
        var x = (char)('A' + vector.X);
        var y = (char)('8' - vector.Y);
        return $"{x}{y}";
    }

    private void HandleInteractions()
    {
        var mouseScreenPos = Raylib.GetMousePosition();
        var mouseGridPos = ScreenToGrid((int)mouseScreenPos.X, (int)mouseScreenPos.Y);
        var isMouseOnBoard = mouseGridPos is { X: >= 0 and < Constants.SquareCount, Y: >= 0 and < Constants.SquareCount };
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            if (isMouseOnBoard)
            {
                isDragging = true;
                mouseGridPos = Vector2.Clamp(mouseGridPos, Vector2.Zero, upperBounds);
                rectBeginPos ??= mouseGridPos;
                rectEndPos = mouseGridPos;
            }
        }
        else if (Raylib.IsMouseButtonUp(MouseButton.MOUSE_BUTTON_LEFT) && isDragging)
        {
            if (rectBeginPos != null && rectEndPos != null)
            {
                boundingBoxes.Add(new HighlightSquare(rectBeginPos.Value, rectEndPos.Value, boundingBoxColor));
                rectBeginPos = null;
                rectEndPos = null;
                boundingBoxColor = Palette.GetNextColor();
            }

            var variables = string.Join("\n", boundingBoxes.Select(GetAreaAsVariable));
            Console.WriteLine("\n### Currently selected areas ###");
            Console.WriteLine(variables);
            Console.Write("\n### As a single bitboard: ");
            var selectionsToBitboard = BoundingBoxesToBitboard(boundingBoxes);
            Console.WriteLine($"0x{Convert.ToString((long)selectionsToBitboard, 16)}");
            isDragging = false;
        }

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)) boundingBoxes.Clear();

        if (rectBeginPos != null && rectEndPos != null)
            DrawRect(rectBeginPos.Value, rectEndPos.Value, boundingBoxColor);
        if (!isMouseOnBoard) return;
        DrawTile(mouseGridPos, boundingBoxColor);
    }

    private void DrawTile(Vector2 tilePosition, Color tileColor)
    {
        DrawTile((int)tilePosition.X, (int)tilePosition.Y, tileColor);
    }

    private void DrawTile(int col, int row, Color tileColor)
    {
        var posX = col * sideLength + halfSideLength;
        var posY = row * sideLength + halfSideLength;
        Raylib.DrawRectangle(posX, posY, sideLength, sideLength, tileColor);
    }

    private void DrawRect(Vector2 rectBeginPos, Vector2 rectEndPos, Color tileColor)
    {
        var isXGreater = rectEndPos.X > rectBeginPos.X;
        var isYGreater = rectEndPos.Y > rectBeginPos.Y;
        for (var x = (int)rectBeginPos.X;
             isXGreater ? x <= rectEndPos.X : x >= rectEndPos.X;
             x += isXGreater ? 1 : -1)
            for (var y = (int)rectBeginPos.Y;
                 isYGreater ? y <= rectEndPos.Y : y >= rectEndPos.Y;
                 y += isYGreater ? 1 : -1)
                DrawTile(x, y, tileColor);
    }

    private readonly struct HighlightSquare(Vector2 startPosition, Vector2 endPosition, Color color)
    {
        internal readonly Color Color = color;
        internal readonly Vector2 StartPosition = startPosition;
        internal readonly Vector2 EndPosition = endPosition;
    }
}