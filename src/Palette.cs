using Color = Raylib_cs.Color;

namespace skakmat;

public abstract class Palette
{
    private static readonly Color[] DistinctColors =
    {
        new(255, 0, 0, 128), // Red
        new(0, 255, 0, 128), // Green
        new(0, 0, 255, 128), // Blue
        new(255, 255, 0, 128), // Yellow
        new(255, 0, 255, 128), // Magenta
        new(0, 255, 255, 128), // Cyan
        new(128, 0, 128, 128), // Purple
        new(255, 165, 0, 128), // Orange
        new(0, 128, 0, 128), // Dark Green
        new(0, 0, 0, 128) // Black
    };

    private static int _colorIndex;

    public static Color GetNextColor()
    {
        if (_colorIndex >= DistinctColors.Length)
            _colorIndex = 0;
        return DistinctColors[_colorIndex++];
    }
}