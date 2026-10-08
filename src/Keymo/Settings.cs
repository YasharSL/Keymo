using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Keymo;

/// <summary>User preferences, kept as a small JSON file. A missing or broken file means defaults.</summary>
internal sealed class Settings
{
    private static readonly KeysConverter KeyNames = new();
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Converters = { new HotkeyJson(), new JsonStringEnumConverter() },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Keeps "+" in hotkeys readable; the file is never put in HTML.
    };

    public static string DefaultPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Keymo", "settings.json");

    public Keys CursorHotkey { get; set; } = Keys.Control | Keys.Alt | Keys.K;

    public Keys GridHotkey { get; set; } = Keys.Control | Keys.Alt | Keys.G;

    /// <summary>Pixels per arrow press.</summary>
    public int SmallStep { get; set; } = 10;

    /// <summary>Pixels per Shift+arrow press.</summary>
    public int BigStep { get; set; } = 100;

    /// <summary>Mouse wheel notches per Alt+arrow press.</summary>
    public int ScrollNotches { get; set; } = 1;

    public AfterGridJump AfterGridJump { get; set; } = AfterGridJump.TurnOnCursorMode;

    /// <summary>A hotkey as people write it, e.g. "Ctrl+Alt+K".</summary>
    public static string HotkeyText(Keys hotkey) => KeyNames.ConvertToInvariantString(hotkey) ?? string.Empty;

    public static Settings Load(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(path), Json) ?? new Settings();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
        {
            return new Settings();
        }
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".");
        File.WriteAllText(path, JsonSerializer.Serialize(this, Json));
    }

    /// <summary>Stores a hotkey as its readable text instead of a number.</summary>
    private sealed class HotkeyJson : JsonConverter<Keys>
    {
        public override Keys Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? text = reader.GetString();
            try
            {
                return (Keys)(KeyNames.ConvertFromInvariantString(text ?? string.Empty) ?? Keys.None);
            }
            catch (Exception e) when (e is FormatException or ArgumentException or NotSupportedException)
            {
                throw new JsonException($"'{text}' is not a hotkey.", e);
            }
        }

        public override void Write(Utf8JsonWriter writer, Keys value, JsonSerializerOptions options) =>
            writer.WriteStringValue(HotkeyText(value));
    }
}
