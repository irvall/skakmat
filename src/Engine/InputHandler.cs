using System.Numerics;
using Raylib_cs;
using skakmat.Game;

namespace skakmat.Engine;
internal class InputHandler(int sideLength)
{

    internal static bool IsLeftArrowPressed => Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT);
    internal static bool IsRightArrowPressed => Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT);
    internal static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed(key);

    internal static bool IsLeftMouseButtonPressed =>
        Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON);

    internal static bool IsMouseOnBoard(Vector2 gridPosition)
    {
        return gridPosition is { X: >= 0 and < Constants.SquareCount, Y: >= 0 and < Constants.SquareCount };
    }

    private readonly int sideLength = sideLength;

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