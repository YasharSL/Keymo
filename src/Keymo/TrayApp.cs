namespace Keymo;

/// <summary>The running app: tray icon and menu, and the routing of every key press to hotkeys and modes.</summary>
internal sealed class TrayApp : ApplicationContext
{
    private const string AppName = "Keymo";

    private readonly Settings _settings = Settings.Load(Settings.DefaultPath);
    private readonly CursorMode _cursorMode;
    private readonly GridOverlay _grid = new();
    private readonly KeyboardHook _hook;
    private readonly NotifyIcon _tray;
    private bool _settingsOpen;

    public TrayApp()
    {
        _cursorMode = new CursorMode(_settings);
        _grid.Jumped += OnGridJumped;
        _tray = new NotifyIcon
        {
            Icon = KeyboardGlyph.ToIcon(),
            Text = AppName,
            ContextMenuStrip = BuildMenu(),
            Visible = true,
        };
        _tray.DoubleClick += (_, _) => ShowSettings();

        // Last, so no key waits on the hook while the rest of startup is still running.
        _hook = new KeyboardHook(OnKeyDown);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hook.Dispose();
            _tray.Dispose();
            _cursorMode.Dispose();
            _grid.Dispose();
        }

        base.Dispose(disposing);
    }

    private bool OnKeyDown(Keys keyData)
    {
        if (keyData == _settings.CursorHotkey)
        {
            _cursorMode.Toggle();
            return true;
        }

        if (keyData == _settings.GridHotkey)
        {
            _grid.Toggle();
            return true;
        }

        return _grid.HandleKey(keyData) || _cursorMode.HandleKey(keyData);
    }

    private void OnGridJumped()
    {
        switch (_settings.AfterGridJump)
        {
            case AfterGridJump.TurnOnCursorMode:
                _cursorMode.TurnOn();
                break;
            case AfterGridJump.Click:
                Input.Click(rightButton: false);
                break;
        }
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Settings…", null, (_, _) => ShowSettings());
        menu.Items.Add("Help", null, (_, _) => ShowMessage(HelpText()));
        menu.Items.Add("Check for updates", null, async (_, _) => ShowMessage(await UpdateCheck.DescribeAsync()));
        menu.Items.Add("About", null, (_, _) => ShowMessage(
            $"{AppName} {UpdateCheck.Current.ToString(3)}\n\nControl the mouse cursor with the keyboard."));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());
        return menu;
    }

    private void ShowSettings()
    {
        if (_settingsOpen)
        {
            return;
        }

        // The hook is paused so the dialog can capture new hotkeys, including the current ones.
        _settingsOpen = _hook.Paused = true;
        using (var form = new SettingsForm(_settings))
        {
            form.ShowDialog();
        }

        _settingsOpen = _hook.Paused = false;
    }

    private string HelpText() => $"""
        Cursor mode: {Settings.HotkeyText(_settings.CursorHotkey)} turns it on and off
            Arrows: move the cursor
            Shift+Arrows: move in big steps
            Alt+Arrows: scroll
            Enter: click, Shift+Enter: right-click
            Esc: turn cursor mode off

        Grid: {Settings.HotkeyText(_settings.GridHotkey)} shows and hides it
            First letter picks the column, second letter picks the row
            Enter: jump to the middle of the cell, then {AfterJumpText()}
            Esc: go one step back, or close the grid
        """;

    private string AfterJumpText() => _settings.AfterGridJump switch
    {
        AfterGridJump.TurnOnCursorMode => "turn on cursor mode",
        AfterGridJump.Click => "click there",
        _ => "do nothing more",
    };

    private static void ShowMessage(string text) =>
        MessageBox.Show(text, AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
}
