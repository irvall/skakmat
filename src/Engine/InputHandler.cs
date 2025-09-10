using System.Numerics;
using Raylib_cs;
using skakmat.Game;

namespace skakmat.Engine;
internal class InputHandler(int sideLength)
{

    internal static bool IsLeftArrowPressed => Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT);
    internal static bool IsRightArrowPressed => Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT);
    internal static bool IsLetterDPressed => Raylib.IsKeyPressed(KeyboardKey.KEY_D);

    internal static bool IsLeftMouseButtonPressed =>
        Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON);

    internal static bool IsMouseOnBoard(Vector2 gridPosition)
    {
        return gridPosition is { X: >= 0 and < Constants.SquareCount, Y: >= 0 and < Constants.SquareCount };
    }

    private readonly int _halfSideLength = sideLength / 2;
    private readonly int _sideLength = sideLength;

    private Vector2 ScreenToGrid(int screenX, int screenY)
    {
        var gridX = (screenX - _halfSideLength) / _sideLength;
        var gridY = (screenY - _halfSideLength) / _sideLength;
        return new Vector2(gridX, gridY);
    }

    internal Vector2 GetMouseGridPosition()
    {
        var mousePos = Raylib.GetMousePosition();
        return ScreenToGrid((int)mousePos.X, (int)mousePos.Y);
    }

}