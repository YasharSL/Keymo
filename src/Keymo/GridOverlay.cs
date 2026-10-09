namespace Keymo;

/// <summary>
/// The see-through 20 x 20 grid over the screen the cursor is on.
/// While shown it takes every key: letters pick a cell, Enter jumps the cursor there.
/// </summary>
internal sealed class GridOverlay : OverlayForm
{
    private const double GridOpacity = 0.55;
    private const float LabelHeightShare = 0.3f;
    private const float MinLabelPixels = 8f;

    private static readonly CellLook Plain = new(Color.FromArgb(48, 48, 52), Color.FromArgb(235, 235, 235));
    private static readonly CellLook Dimmed = new(Color.FromArgb(8, 8, 8), Color.FromArgb(120, 120, 120));
    private static readonly CellLook InPickedColumn = new(Color.FromArgb(40, 90, 160), Color.White);
    private static readonly CellLook Picked = new(Color.FromArgb(255, 196, 0), Color.Black);
    private static readonly Color Lines = Color.FromArgb(150, 150, 150);

    private readonly GridSelection _selection = new();

    public GridOverlay()
        : base(GridOpacity)
    {
        BackColor = Color.Black;
    }

    /// <summary>Raised after Enter has moved the cursor to the picked cell and the grid has closed.</summary>
    public event Action? Jumped;

    public void Toggle()
    {
        if (Visible)
        {
            Hide();
            return;
        }

        _selection.Reset();
        Rectangle screen = Screen.FromPoint(Cursor.Position).Bounds;
        Bounds = screen;
        Show();
        Bounds = screen; // Again, in case showing on a monitor with another DPI resized the window.
    }

    /// <summary>Takes the key if the grid is showing. Returns whether it did.</summary>
    public bool HandleKey(Keys keyData)
    {
        if (!Visible)
        {
            return false;
        }

        switch (_selection.Press(keyData & Keys.KeyCode))
        {
            case GridKeyResult.Changed:
                Invalidate();
                break;
            case GridKeyResult.Dismissed:
                Hide();
                break;
            case GridKeyResult.Committed when _selection.Target(Bounds) is Point target:
                Input.MoveTo(target);
                Hide();
                Jumped?.Invoke();
                break;
        }

        return true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Rectangle area = ClientRectangle;
        float labelPixels = Math.Max(MinLabelPixels, area.Height / GridSelection.Size * LabelHeightShare);
        using var font = new Font("Segoe UI", labelPixels, FontStyle.Bold, GraphicsUnit.Pixel);
        using var lines = new Pen(Lines);
        for (int column = 0; column < GridSelection.Size; column++)
        {
            for (int row = 0; row < GridSelection.Size; row++)
            {
                PaintCell(e.Graphics, GridSelection.CellBounds(area, column, row), column, row, font, lines);
            }
        }
    }

    private void PaintCell(Graphics graphics, Rectangle cell, int column, int row, Font font, Pen lines)
    {
        CellLook look = LookOf(column, row);
        using var fill = new SolidBrush(look.Fill);
        graphics.FillRectangle(fill, cell);
        graphics.DrawRectangle(lines, cell);
        string label = $"{GridSelection.LetterOf(column)}{GridSelection.LetterOf(row)}";
        const TextFormatFlags Centered =
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;
        TextRenderer.DrawText(graphics, label, font, cell, look.Text, Centered);
    }

    private CellLook LookOf(int column, int row)
    {
        if (_selection.Column is not int pickedColumn)
        {
            return Plain;
        }

        if (column != pickedColumn)
        {
            return Dimmed;
        }

        return row == _selection.Row ? Picked : InPickedColumn;
    }

    private readonly record struct CellLook(Color Fill, Color Text);
}
