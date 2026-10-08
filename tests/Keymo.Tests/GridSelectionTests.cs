namespace Keymo.Tests;

public class GridSelectionTests
{
    private static readonly Rectangle Screen = new(0, 0, 2000, 1000);

    [Fact]
    public void First_letter_picks_column_second_picks_row()
    {
        var selection = new GridSelection();

        Assert.Equal(GridKeyResult.Changed, selection.Press(Keys.C));
        Assert.Equal((2, null), (selection.Column, selection.Row));

        Assert.Equal(GridKeyResult.Changed, selection.Press(Keys.T));
        Assert.Equal((2, 19), (selection.Column, selection.Row));
    }

    [Theory]
    [InlineData(Keys.U)] // 21st letter: outside a 20-wide grid
    [InlineData(Keys.D1)]
    [InlineData(Keys.Space)]
    public void Keys_outside_the_grid_letters_are_ignored(Keys key)
    {
        var selection = new GridSelection();

        Assert.Equal(GridKeyResult.Ignored, selection.Press(key));
        Assert.Null(selection.Column);
    }

    [Fact]
    public void Third_letter_is_ignored()
    {
        var selection = Picked(Keys.A, Keys.B);

        Assert.Equal(GridKeyResult.Ignored, selection.Press(Keys.C));
        Assert.Equal((0, 1), (selection.Column, selection.Row));
    }

    [Fact]
    public void Escape_steps_back_row_then_column_then_dismisses()
    {
        var selection = Picked(Keys.A, Keys.B);

        Assert.Equal(GridKeyResult.Changed, selection.Press(Keys.Escape));
        Assert.Equal((0, null), (selection.Column, selection.Row));

        Assert.Equal(GridKeyResult.Changed, selection.Press(Keys.Escape));
        Assert.Null(selection.Column);

        Assert.Equal(GridKeyResult.Dismissed, selection.Press(Keys.Escape));
    }

    [Fact]
    public void Enter_commits_only_a_full_pick()
    {
        var selection = new GridSelection();
        Assert.Equal(GridKeyResult.Ignored, selection.Press(Keys.Enter));

        selection.Press(Keys.A);
        Assert.Equal(GridKeyResult.Ignored, selection.Press(Keys.Enter));

        selection.Press(Keys.A);
        Assert.Equal(GridKeyResult.Committed, selection.Press(Keys.Enter));
    }

    [Fact]
    public void Target_is_the_middle_of_the_picked_cell()
    {
        // Cells are 100 x 50 here; column B, row C is x 100..200, y 100..150.
        Assert.Equal(new Point(150, 125), Picked(Keys.B, Keys.C).Target(Screen));
    }

    [Fact]
    public void Target_respects_a_screen_that_does_not_start_at_zero()
    {
        var secondMonitor = new Rectangle(-2000, 300, 2000, 1000);

        Assert.Equal(new Point(-1950, 325), Picked(Keys.A, Keys.A).Target(secondMonitor));
    }

    [Fact]
    public void Target_is_null_until_both_letters_are_picked()
    {
        var selection = new GridSelection();
        selection.Press(Keys.A);

        Assert.Null(selection.Target(Screen));
    }

    [Fact]
    public void Cells_tile_an_area_that_does_not_divide_evenly()
    {
        var area = new Rectangle(0, 0, 1366, 768);

        Rectangle last = GridSelection.CellBounds(area, GridSelection.Size - 1, GridSelection.Size - 1);
        Rectangle beforeLast = GridSelection.CellBounds(area, GridSelection.Size - 2, GridSelection.Size - 2);

        Assert.Equal((area.Right, area.Bottom), (last.Right, last.Bottom));
        Assert.Equal((beforeLast.Right, beforeLast.Bottom), (last.Left, last.Top));
    }

    [Fact]
    public void Reset_clears_the_pick()
    {
        var selection = Picked(Keys.A, Keys.B);

        selection.Reset();

        Assert.Equal((null, null), (selection.Column, selection.Row));
    }

    private static GridSelection Picked(Keys column, Keys row)
    {
        var selection = new GridSelection();
        selection.Press(column);
        selection.Press(row);
        return selection;
    }
}
