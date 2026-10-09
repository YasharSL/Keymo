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
    private bool? _heldButtonIsRight; // Null while no mouse button is held down.
    private bool _dragLocked; // The held button stays down after Enter comes up.
    private bool _enterIsDown; // Tells a fresh Enter press from key repeats.

    public CursorMode(Settings settings)
    {
        _settings = settings;
        _follow.Tick += (_, _) => _badge.MoveToCursor();
    }

    public void Toggle()
    {
        if (_badge.Visible)
        {
            ReleaseButton();
            _follow.Stop();
            _badge.Hide();
            return;
        }

        TurnOn();
    }

    /// <summary>Activates the mode; does nothing if it is already active.</summary>
    public void TurnOn()
    {
        // Counts as mouse input, which makes Windows show a cursor it hid after touch use or sleep.
        Input.MoveTo(Cursor.Position);
        _badge.MoveToCursor();
        _badge.Show();
        _follow.Start();
    }

    /// <summary>
    /// Tells the mode that Enter is down for another reason (confirming the grid),
    /// so it must not start a click until Enter has come up.
    /// </summary>
    public void IgnoreHeldEnter() => _enterIsDown = true;

    /// <summary>Acts on the key press if the mode is active and the key is one of its own. Returns whether it did.</summary>
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
            PressEnter(rightButton: keyData.HasFlag(Keys.Shift), lockDrag: keyData.HasFlag(Keys.Alt));
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
            Input.MoveTo(Cursor.Position + new Size(direction.Width * step, direction.Height * step));
            _badge.MoveToCursor();
        }

        return true;
    }

    /// <summary>Releases an unlocked mouse button when Enter comes up. Returns whether the Enter press was this mode's.</summary>
    public bool HandleKeyUp(Keys keyData)
    {
        if ((keyData & Keys.KeyCode) != Keys.Enter)
        {
            return false;
        }

        bool wasDown = _enterIsDown;
        _enterIsDown = false;
        if (!_dragLocked)
        {
            ReleaseButton();
        }

        return wasDown;
    }

    public void Dispose()
    {
        ReleaseButton();
        _follow.Dispose();
        _badge.Dispose();
    }

    // Enter holds the button for as long as it is down, so arrows in between drag.
    // With lockDrag (Alt+Enter) the button stays down until the next Enter or Alt+Enter, for keyboards
    // that cannot hold Enter and reach the arrows at once. Key repeats change nothing.
    private void PressEnter(bool rightButton, bool lockDrag)
    {
        if (_enterIsDown)
        {
            return;
        }

        _enterIsDown = true;
        if (_heldButtonIsRight is not null)
        {
            ReleaseButton(); // Only a locked drag is still held at a fresh press: this drops it.
            return;
        }

        _heldButtonIsRight = rightButton;
        _dragLocked = lockDrag;
        Input.SetButton(rightButton, down: true);
    }

    private void ReleaseButton()
    {
        if (_heldButtonIsRight is bool rightButton)
        {
            _heldButtonIsRight = null;
            _dragLocked = false;
            Input.SetButton(rightButton, down: false);
        }
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
