using System.Numerics;
using Raylib_cs;

namespace skakmat;
public class InputHandler(int sideLength)
{
    public static bool IsLeftMouseButtonPressed =>
        Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON);

    public static bool IsMouseOnBoard(Vector2 gridPosition)
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

    public Vector2 GetMouseGridPosition()
    {
        var mousePos = Raylib.GetMousePosition();
        return ScreenToGrid((int)mousePos.X, (int)mousePos.Y);
    }

}