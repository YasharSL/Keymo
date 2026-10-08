namespace Keymo;

/// <summary>The little keyboard picture shown next to the cursor and in the tray.</summary>
internal static class KeyboardGlyph
{
    private const int KeysPerRow = 4;
    private const int KeyRows = 2;
    private const int IconSize = 32;

    private static readonly Color Body = Color.FromArgb(32, 32, 36);
    private static readonly Color Keycaps = Color.White;

    /// <summary>Draws a keyboard filling <paramref name="box"/>: a dark body, two rows of keys and a space bar.</summary>
    public static void Draw(Graphics graphics, Rectangle box)
    {
        using var body = new SolidBrush(Body);
        using var keycaps = new SolidBrush(Keycaps);
        using var outline = new Pen(Keycaps);
        graphics.FillRectangle(body, box);
        graphics.DrawRectangle(outline, box.X, box.Y, box.Width - 1, box.Height - 1);

        // Everything is laid out on an 11 x 10 unit design grid scaled to the box.
        float unitX = box.Width / 11f;
        float unitY = box.Height / 10f;
        for (int row = 0; row < KeyRows; row++)
        {
            for (int key = 0; key < KeysPerRow; key++)
            {
                graphics.FillRectangle(
                    keycaps,
                    box.X + (unitX * (1.5f + (2.2f * key))),
                    box.Y + (unitY * (2f + (2.2f * row))),
                    unitX * 1.4f,
                    unitY * 1.4f);
            }
        }

        graphics.FillRectangle(keycaps, box.X + (unitX * 3f), box.Y + (unitY * 6.6f), unitX * 5f, unitY * 1.4f);
    }

    /// <summary>The glyph as a tray icon. Created once; its handle lives as long as the process.</summary>
    public static Icon ToIcon()
    {
        using var bitmap = new Bitmap(IconSize, IconSize);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            Draw(graphics, new Rectangle(1, 6, IconSize - 2, IconSize - 12));
        }

        return Icon.FromHandle(bitmap.GetHicon());
    }
}
