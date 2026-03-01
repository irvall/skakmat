using System.Numerics;
using Raylib_cs;
using skakmat.Game;

namespace skakmat.Engine;

internal class InputHandler(int sideLength)
{
    private readonly int sideLength = sideLength;

    internal static bool IsLeftArrowPressed => Raylib.IsKeyPressed(KeyboardKey.Left);
    internal static bool IsRightArrowPressed => Raylib.IsKeyPressed(KeyboardKey.Right);

    internal static bool IsLeftMouseButtonPressed =>
        Raylib.IsMouseButtonPressed(MouseButton.Left);

    internal static bool IsKeyPressed(KeyboardKey key)
    {
        return Raylib.IsKeyPressed(key);
    }

    internal static bool IsMouseOnBoard(Vector2 gridPosition)
    {
        return gridPosition is { X: >= 0 and < Constants.SquareCount, Y: >= 0 and < Constants.SquareCount };
    }

    private Vector2 ScreenToGrid(int screenX, int screenY)
    {
        var gridX = screenX / sideLength;
        var gridY = screenY / sideLength;
        return new Vector2(gridX, gridY);
    }

    internal Vector2 GetMouseGridPosition()
    {
        var mousePos = Raylib.GetMousePosition();
        return ScreenToGrid((int)mousePos.X, (int)mousePos.Y);
    }
}