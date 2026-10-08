namespace Keymo;

/// <summary>The settings dialog. OK writes the values into the given settings and saves them.</summary>
internal sealed class SettingsForm : Form
{
    private const int MaxStepPixels = 2000;
    private const int MaxScrollNotches = 20;
    private const int FieldWidth = 140;

    private readonly Settings _settings;
    private readonly TableLayoutPanel _rows = new() { AutoSize = true, ColumnCount = 2, Dock = DockStyle.Fill };
    private readonly TextBox _cursorHotkey;
    private readonly TextBox _gridHotkey;
    private readonly NumericUpDown _smallStep;
    private readonly NumericUpDown _bigStep;
    private readonly NumericUpDown _scrollNotches;

    public SettingsForm(Settings settings)
    {
        _settings = settings;
        Text = "Keymo settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = MinimizeBox = ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        _cursorHotkey = AddHotkey("Cursor mode hotkey", settings.CursorHotkey);
        _gridHotkey = AddHotkey("Grid hotkey", settings.GridHotkey);
        _smallStep = AddNumber("Arrow step (pixels)", settings.SmallStep, MaxStepPixels);
        _bigStep = AddNumber("Shift+Arrow step (pixels)", settings.BigStep, MaxStepPixels);
        _scrollNotches = AddNumber("Scroll speed (wheel notches)", settings.ScrollNotches, MaxScrollNotches);
        AddButtons();
        Controls.Add(_rows);
    }

    private TextBox AddHotkey(string label, Keys hotkey)
    {
        var box = new TextBox { ReadOnly = true, Width = FieldWidth, Tag = hotkey, Text = Settings.HotkeyText(hotkey) };
        box.KeyDown += (_, e) =>
        {
            e.SuppressKeyPress = true;
            bool isModifierOnly = e.KeyCode is Keys.ControlKey or Keys.ShiftKey or Keys.Menu;

            // Ctrl or Alt is required so a hotkey can never be a key people type.
            if (!isModifierOnly && (e.Control || e.Alt))
            {
                box.Tag = e.KeyData;
                box.Text = Settings.HotkeyText(e.KeyData);
            }
        };
        AddRow(label, box);
        return box;
    }

    private NumericUpDown AddNumber(string label, int value, int maximum)
    {
        var number = new NumericUpDown { Minimum = 1, Maximum = maximum, Width = FieldWidth };
        number.Value = Math.Clamp(value, 1, maximum);
        AddRow(label, number);
        return number;
    }

    private void AddRow(string label, Control field)
    {
        _rows.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left });
        _rows.Controls.Add(field);
    }

    private void AddButtons()
    {
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        ok.Click += (_, _) => Apply();
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(ok);
        _rows.Controls.Add(buttons);
        _rows.SetColumnSpan(buttons, 2);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private void Apply()
    {
        _settings.CursorHotkey = (Keys)_cursorHotkey.Tag!; // Tag is set to a Keys value where each box is created.
        _settings.GridHotkey = (Keys)_gridHotkey.Tag!;
        _settings.SmallStep = (int)_smallStep.Value;
        _settings.BigStep = (int)_bigStep.Value;
        _settings.ScrollNotches = (int)_scrollNotches.Value;
        try
        {
            _settings.Save(Settings.DefaultPath);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"The settings apply now but could not be saved:\n{e.Message}", Text);
        }
    }
}
