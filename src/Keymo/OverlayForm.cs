using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace Keymo;

/// <summary>
/// A window that floats above everything without ever taking focus or mouse clicks,
/// so the app the user is working in stays active underneath it.
/// </summary>
internal abstract class OverlayForm : Form
{
    private const int TopmostStyle = 0x00000008;
    private const int ClickThroughStyle = 0x00000020;
    private const int ToolWindowStyle = 0x00000080;
    private const int NoActivateStyle = 0x08000000;

    private const int StayOnTopIntervalMs = 200;
    private const uint KeepSizePositionAndFocus = 0x0001 | 0x0002 | 0x0010; // NOSIZE | NOMOVE | NOACTIVATE
    private static readonly IntPtr AboveAllWindows = -1; // HWND_TOPMOST

    private readonly Timer _stayOnTop = new() { Interval = StayOnTopIntervalMs };

    protected OverlayForm(double opacity)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        AutoScaleMode = AutoScaleMode.None;
        ShowInTaskbar = false;
        DoubleBuffered = true;
        Opacity = opacity; // Below 1 makes the window layered, which click-through needs.
        _stayOnTop.Tick += (_, _) => RaiseAboveAll();
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            // Topmost goes here because the TopMost property activates the window on show.
            CreateParams parameters = base.CreateParams;
            parameters.ExStyle |= TopmostStyle | ClickThroughStyle | ToolWindowStyle | NoActivateStyle;
            return parameters;
        }
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        // The style alone does not hold: windows opened or raised later can end up above the overlay,
        // so while it shows it keeps putting itself back on top.
        _stayOnTop.Enabled = Visible;
        if (Visible)
        {
            RaiseAboveAll();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stayOnTop.Dispose();
        }

        base.Dispose(disposing);
    }

    // A failed call only leaves the overlay where it was until the next tick, so the result is not checked.
    private void RaiseAboveAll() => _ = SetWindowPos(Handle, AboveAllWindows, 0, 0, 0, 0, KeepSizePositionAndFocus);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);
}
