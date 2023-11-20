using Raylib_cs;

public static class ColorUtils
{
    public static Color BrightenUp(this Color originalColor, float factor)
    {
        // Ensure the factor is within a valid range (0 to 1)
        factor = Math.Clamp(factor, 0f, 1f);

        // Calculate the new color components
        var newR = (byte)Math.Clamp(originalColor.R + (255 - originalColor.R) * factor, 0, 255);
        var newG = (byte)Math.Clamp(originalColor.G + (255 - originalColor.G) * factor, 0, 255);
        var newB = (byte)Math.Clamp(originalColor.B + (255 - originalColor.B) * factor, 0, 255);

        // Return the new Color
        return new Color(newR, newG, newB, originalColor.A);
    }
}