using System.Numerics;
using Raylib_cs;

internal class MouseController
{
    public Vector2 MousePosition { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the left mouse button is currently pressed.
    /// </summary>
    public static bool LeftMouseButtonPressed => Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON);

    public static bool LeftMouseButtonReleased => Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON);

    public static bool RightMouseButtonPressed => Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON);
    public static bool RightMouseButtonReleased => Raylib.IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON);

    public void Update()
    {
        MousePosition = Raylib.GetMousePosition();
    }
}