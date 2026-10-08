namespace Keymo.Tests;

public sealed class SettingsTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "Keymo.Tests." + Guid.NewGuid().ToString("N"));

    private string FilePath => Path.Combine(_folder, "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, recursive: true);
        }
    }

    [Fact]
    public void Missing_file_gives_defaults()
    {
        Settings settings = Settings.Load(FilePath);

        Assert.Equal(Keys.Control | Keys.Alt | Keys.K, settings.CursorHotkey);
        Assert.Equal(Keys.Control | Keys.Alt | Keys.G, settings.GridHotkey);
        Assert.Equal((10, 100, 1), (settings.SmallStep, settings.BigStep, settings.ScrollNotches));
    }

    [Fact]
    public void Saved_settings_load_back_the_same()
    {
        var saved = new Settings
        {
            CursorHotkey = Keys.Alt | Keys.Shift | Keys.F9,
            GridHotkey = Keys.Control | Keys.Oem3,
            SmallStep = 3,
            BigStep = 250,
            ScrollNotches = 4,
        };

        saved.Save(FilePath);
        Settings loaded = Settings.Load(FilePath);

        Assert.Equal(saved.CursorHotkey, loaded.CursorHotkey);
        Assert.Equal(saved.GridHotkey, loaded.GridHotkey);
        Assert.Equal((3, 250, 4), (loaded.SmallStep, loaded.BigStep, loaded.ScrollNotches));
    }

    [Fact]
    public void Hotkeys_are_stored_as_readable_text()
    {
        new Settings().Save(FilePath);

        Assert.Contains("\"Ctrl+Alt+K\"", File.ReadAllText(FilePath));
    }

    [Theory]
    [InlineData("not json at all")]
    [InlineData("""{ "CursorHotkey": "Ctrl+Alt+NoSuchKey" }""")]
    [InlineData("""{ "SmallStep": "ten" }""")]
    public void Broken_file_gives_defaults(string content)
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(FilePath, content);

        Settings settings = Settings.Load(FilePath);

        Assert.Equal(Keys.Control | Keys.Alt | Keys.K, settings.CursorHotkey);
        Assert.Equal(10, settings.SmallStep);
    }

    [Fact]
    public void Hotkey_text_reads_like_a_shortcut()
    {
        Assert.Equal("Ctrl+Alt+G", Settings.HotkeyText(Keys.Control | Keys.Alt | Keys.G));
    }
}
