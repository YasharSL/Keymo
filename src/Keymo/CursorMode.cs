using Timer = System.Windows.Forms.Timer;

namespace Keymo;

/// <summary>
/// Keyboard control of the cursor: while active, a keyboard badge follows the cursor
/// and the arrow keys move it, scroll, or click.
/// </summary>
internal sealed class CursorMode : IDisposable
{
    private const int FollowIntervalMs = 15;

    private readonly Settings _settings;
    private readonly Badge _badge = new();
    private readonly Timer _follow = new() { Interval = FollowIntervalMs };

    public CursorMode(Settings settings)
    {
        _settings = settings;
        _follow.Tick += (_, _) => _badge.MoveToCursor();
    }

    public void Toggle()
    {
        if (_badge.Visible)
        {
            _follow.Stop();
            _badge.Hide();
            return;
        }

        TurnOn();
    }

    /// <summary>Activates the mode; does nothing if it is already active.</summary>
    public void TurnOn()
    {
        _badge.MoveToCursor();
        _badge.Show();
        _follow.Start();
    }

    /// <summary>Acts on the key if the mode is active and the key is one of its own. Returns whether it did.</summary>
    public bool HandleKey(Keys keyData)
    {
        if (!_badge.Visible)
        {
            return false;
        }

        Keys key = keyData & Keys.KeyCode;
        if (key == Keys.Escape)
        {
            Toggle();
            return true;
        }

        if (key == Keys.Enter)
        {
            Input.Click(rightButton: keyData.HasFlag(Keys.Shift));
            return true;
        }

        if (DirectionOf(key) is not Size direction)
        {
            return false;
        }

        if (keyData.HasFlag(Keys.Alt))
        {
            int notches = _settings.ScrollNotches;
            Input.Scroll(direction.Width * notches, -direction.Height * notches);
        }
        else
        {
            int step = keyData.HasFlag(Keys.Shift) ? _settings.BigStep : _settings.SmallStep;
            Cursor.Position += new Size(direction.Width * step, direction.Height * step);
            _badge.MoveToCursor();
        }

        return true;
    }

    public void Dispose()
    {
        _follow.Dispose();
        _badge.Dispose();
    }

    private static Size? DirectionOf(Keys key) => key switch
    {
        Keys.Left => new Size(-1, 0),
        Keys.Right => new Size(1, 0),
        Keys.Up => new Size(0, -1),
        Keys.Down => new Size(0, 1),
        _ => null,
    };

    /// <summary>The tiny keyboard picture that sits just below and right of the cursor.</summary>
    private sealed class Badge : OverlayForm
    {
        private const double BadgeOpacity = 0.9;
        private const int StandardDpi = 96;
        private static readonly Size BaseSize = new(22, 14);
        private static readonly Size BaseOffset = new(14, 18);

        public Badge()
            : base(BadgeOpacity)
        {
        }

        public void MoveToCursor()
        {
            int dpi = DeviceDpi;
            Size = Scale(BaseSize, dpi);
            Location = Cursor.Position + Scale(BaseOffset, dpi);
        }

        protected override void OnPaint(PaintEventArgs e) => KeyboardGlyph.Draw(e.Graphics, ClientRectangle);

        private static Size Scale(Size size, int dpi) => new(size.Width * dpi / StandardDpi, size.Height * dpi / StandardDpi);
    }
}
