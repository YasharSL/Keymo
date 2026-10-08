using System.Runtime.InteropServices;

namespace Keymo.Tests;

public class OverlayFormTests
{
    private const int ExtendedStyleIndex = -20;
    private const int TopmostStyle = 0x00000008;

    [Fact]
    public void Shown_overlay_is_above_all_windows_and_does_not_take_focus()
    {
        using var overlay = new Probe();

        overlay.Show();

        Assert.NotEqual(0, GetWindowLong(overlay.Handle, ExtendedStyleIndex) & TopmostStyle);
        Assert.NotEqual(overlay.Handle, GetForegroundWindow());
    }

    [Fact]
    public void Overlay_is_still_above_all_windows_when_shown_again()
    {
        using var overlay = new Probe();
        overlay.Show();
        overlay.Hide();

        overlay.Show();

        Assert.NotEqual(0, GetWindowLong(overlay.Handle, ExtendedStyleIndex) & TopmostStyle);
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr window, int index);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    /// <summary>A one-pixel overlay far off screen, so the test shows nothing.</summary>
    private sealed class Probe : OverlayForm
    {
        public Probe()
            : base(opacity: 0.5)
        {
            Bounds = new Rectangle(-32000, -32000, 1, 1);
        }
    }
}
