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

    protected OverlayForm(double opacity)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        AutoScaleMode = AutoScaleMode.None;
        ShowInTaskbar = false;
        DoubleBuffered = true;
        Opacity = opacity; // Below 1 makes the window layered, which click-through needs.
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
}
