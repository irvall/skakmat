using Color = Raylib_cs.Color;

namespace skakmat.Rendering;

internal abstract class Palette
{
    private static readonly Color[] DistinctColors =
    [
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
    ];

    private static int _colorIndex;

    internal static Color GetNextColor()
    {
        if (_colorIndex >= DistinctColors.Length)
            _colorIndex = 0;
        return DistinctColors[_colorIndex++];
    }

    internal static Color FromHex(string hex)
    {
        if (hex.StartsWith('#'))
            hex = hex[1..];

        byte r = byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color(r, g, b, (byte)255);
    }

    internal static Color WhiteVersion(Color c)
    {
        // Find the maximum of the RGB channels
        byte max = Math.Max(c.R, Math.Max(c.G, c.B));

        if (max == 0)
        {
            // Avoid divide-by-zero: if it's black, return white
            return new Color((byte)255, (byte)255, (byte)255, c.A);
        }

        // Scale factor to push the max channel up to 255
        float scale = 255f / max;

        // Scale each channel evenly
        byte r = (byte)Math.Min(255, (int)(c.R * scale));
        byte g = (byte)Math.Min(255, (int)(c.G * scale));
        byte b = (byte)Math.Min(255, (int)(c.B * scale));

        return new Color(r, g, b, c.A);
    }

    public static class Coolors
    {
        public static Color ForestGreen = FromHex("4D8B31");
        public static Color Tea = FromHex("C7D59F");
        public static Color RoseQuartz = FromHex("AEA4BF");
    }

}