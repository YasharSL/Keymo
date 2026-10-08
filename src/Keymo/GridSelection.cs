namespace Keymo;

/// <summary>What a key press did to the grid selection.</summary>
internal enum GridKeyResult
{
    Ignored,
    Changed,
    Dismissed,
    Committed,
}

/// <summary>
/// The grid's two-step pick: a letter chooses the column, the next letter chooses the row,
/// Esc undoes one step (or dismisses the grid when nothing is picked), Enter commits a full pick.
/// </summary>
internal sealed class GridSelection
{
    /// <summary>Number of columns and of rows; letters A.. label both.</summary>
    public const int Size = 20;

    public int? Column { get; private set; }

    public int? Row { get; private set; }

    public static Rectangle CellBounds(Rectangle area, int column, int row)
    {
        // Edges are computed from the whole area so cells tile it exactly, with no rounding gap at the far side.
        int left = area.Left + (area.Width * column / Size);
        int top = area.Top + (area.Height * row / Size);
        int right = area.Left + (area.Width * (column + 1) / Size);
        int bottom = area.Top + (area.Height * (row + 1) / Size);
        return Rectangle.FromLTRB(left, top, right, bottom);
    }

    public static char LetterOf(int index) => (char)('A' + index);

    public void Reset() => Column = Row = null;

    public GridKeyResult Press(Keys key) => key switch
    {
        Keys.Escape => StepBack(),
        Keys.Enter => Row is null ? GridKeyResult.Ignored : GridKeyResult.Committed,
        _ => Pick(key - Keys.A),
    };

    /// <summary>The middle of the picked cell within <paramref name="area"/>, or null until both letters are picked.</summary>
    public Point? Target(Rectangle area)
    {
        if (Column is not int column || Row is not int row)
        {
            return null;
        }

        Rectangle cell = CellBounds(area, column, row);
        return new Point(cell.Left + (cell.Width / 2), cell.Top + (cell.Height / 2));
    }

    private GridKeyResult StepBack()
    {
        if (Row is not null)
        {
            Row = null;
        }
        else if (Column is not null)
        {
            Column = null;
        }
        else
        {
            return GridKeyResult.Dismissed;
        }

        return GridKeyResult.Changed;
    }

    private GridKeyResult Pick(int index)
    {
        if (index is < 0 or >= Size || Row is not null)
        {
            return GridKeyResult.Ignored;
        }

        if (Column is null)
        {
            Column = index;
        }
        else
        {
            Row = index;
        }

        return GridKeyResult.Changed;
    }
}
