using System.Text.Json;

namespace Keymo;

/// <summary>Compares this build with the latest published release.</summary>
internal static class UpdateCheck
{
    private const string LatestReleaseUrl = "https://api.github.com/repos/YasharSL/Keymo/releases/latest";
    private const string DownloadPage = "https://github.com/YasharSL/Keymo/releases/latest";

    public static Version Current { get; } = typeof(UpdateCheck).Assembly.GetName().Version ?? new Version(0, 0, 0);

    /// <summary>A sentence for the user saying whether a newer version exists.</summary>
    public static async Task<string> DescribeAsync()
    {
        string current = Current.ToString(3);
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("Keymo");
            Version latest = ParseLatest(await http.GetStringAsync(LatestReleaseUrl));
            return latest > Current
                ? $"Keymo {latest} is available. You have {current}.\n\nGet it at {DownloadPage}"
                : $"Keymo {current} is the latest version.";
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException or FormatException)
        {
            return $"Could not check for updates:\n{e.Message}";
        }
    }

    /// <summary>Reads the version from a GitHub release document (its "tag_name", with or without a leading v).</summary>
    internal static Version ParseLatest(string releaseJson)
    {
        using JsonDocument release = JsonDocument.Parse(releaseJson);
        string tag = release.RootElement.TryGetProperty("tag_name", out JsonElement name) ? name.GetString() ?? string.Empty : string.Empty;
        return Version.TryParse(tag.TrimStart('v', 'V'), out Version? version)
            ? version
            : throw new FormatException($"Release tag '{tag}' is not a version.");
    }
}
