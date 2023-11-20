using Raylib_cs;

internal class KeyController
{
    public static bool LeftArrowPressed => Raylib.IsKeyDown(KeyboardKey.KEY_LEFT);
    public static bool RightArrowPressed => Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT);
}