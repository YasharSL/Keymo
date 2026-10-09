using System.Runtime.InteropServices;

namespace Keymo;

/// <summary>Injects mouse wheel, mouse button and key events as if the user made them.</summary>
internal static class Input
{
    private const int WheelNotch = 120;
    private const uint MouseType = 0;
    private const uint KeyboardType = 1;
    private const int AbsoluteRange = 65536;
    private const uint MoveFlag = 0x0001;
    private const uint WholeDesktopFlag = 0x4000;
    private const uint AbsoluteFlag = 0x8000;
    private const uint LeftDown = 0x0002;
    private const uint LeftUp = 0x0004;
    private const uint RightDown = 0x0008;
    private const uint RightUp = 0x0010;
    private const uint VerticalWheel = 0x0800;
    private const uint HorizontalWheel = 0x1000;
    private const uint KeyUp = 0x0002;
    private const uint ExtendedKey = 0x0001;

    /// <summary>
    /// A key code no keyboard has. Tapping it between an Alt press and release
    /// stops the focused app from opening its menu bar on the release.
    /// </summary>
    public const Keys AltMenuMaskKey = (Keys)0xE8;

    /// <summary>
    /// Scrolls by wheel notches. Positive is right and up.
    /// The app always gets a plain wheel turn: a held Alt is lifted first, because apps give
    /// Alt+wheel meanings of their own (sideways scrolling, fast scrolling) or ignore it.
    /// Alt then stays lifted for Windows until the user presses it again.
    /// </summary>
    public static void Scroll(int rightNotches, int upNotches)
    {
        List<NativeInput> inputs = HeldAltLift();
        if (rightNotches != 0)
        {
            inputs.Add(Mouse(HorizontalWheel, rightNotches * WheelNotch));
        }

        if (upNotches != 0)
        {
            inputs.Add(Mouse(VerticalWheel, upNotches * WheelNotch));
        }

        Send([.. inputs]);
    }

    public static void Click(bool rightButton)
    {
        SetButton(rightButton, down: true);
        SetButton(rightButton, down: false);
    }

    /// <summary>
    /// Presses or releases a mouse button and leaves it that way.
    /// Like the wheel, the button arrives plain: a held Alt is lifted first, so it is not an Alt+click.
    /// </summary>
    public static void SetButton(bool rightButton, bool down)
    {
        uint flags = rightButton ? (down ? RightDown : RightUp) : (down ? LeftDown : LeftUp);
        List<NativeInput> inputs = HeldAltLift();
        inputs.Add(Mouse(flags));
        Send([.. inputs]);
    }

    /// <summary>
    /// Puts the cursor on an exact screen position, as real mouse input. Setting the position alone is not
    /// input: Windows keeps a cursor hidden that it hid after touch use or sleep, and some apps ignore it mid-drag.
    /// </summary>
    public static void MoveTo(Point target)
    {
        Rectangle desktop = SystemInformation.VirtualScreen;
        target.X = Math.Clamp(target.X, desktop.Left, desktop.Right - 1);
        target.Y = Math.Clamp(target.Y, desktop.Top, desktop.Bottom - 1);
        Cursor.Position = target;

        NativeInput move = Mouse(MoveFlag | AbsoluteFlag | WholeDesktopFlag);
        move.Event.Mouse.X = ToAbsolute(target.X - desktop.Left, desktop.Width);
        move.Event.Mouse.Y = ToAbsolute(target.Y - desktop.Top, desktop.Height);
        Send(move);
    }

    /// <summary>
    /// A pixel offset as the 0..65536 fraction of the desktop that absolute mouse input uses.
    /// Rounded up, so that Windows scaling it back down lands on the same pixel.
    /// </summary>
    internal static int ToAbsolute(int offset, int size) => (int)((((long)offset * AbsoluteRange) + size - 1) / size);

    public static void TapKey(Keys key) => Send(Key(key, 0), Key(key, KeyUp));

    // Key-ups for every Alt that is down, each preceded by the mask key so the release opens no menu bar.
    private static List<NativeInput> HeldAltLift()
    {
        List<NativeInput> inputs = [];
        foreach (Keys alt in (Keys[])[Keys.LMenu, Keys.RMenu])
        {
            if (GetAsyncKeyState((int)alt) < 0)
            {
                inputs.Add(Key(AltMenuMaskKey, 0));
                inputs.Add(Key(AltMenuMaskKey, KeyUp));
                inputs.Add(Key(alt, KeyUp | (alt == Keys.RMenu ? ExtendedKey : 0)));
            }
        }

        return inputs;
    }

    private static NativeInput Mouse(uint flags, int data = 0) =>
        new() { Type = MouseType, Event = { Mouse = { Flags = flags, Data = data } } };

    private static NativeInput Key(Keys key, uint flags) =>
        new() { Type = KeyboardType, Event = { Keyboard = { KeyCode = (ushort)key, Flags = flags } } };

    // A failed injection only loses one click or scroll step, so the result is not checked.
    private static void Send(params NativeInput[] inputs) =>
        _ = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeInput>());

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int key);

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint count, NativeInput[] inputs, int size);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeInput
    {
        public uint Type;
        public InputEvent Event;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputEvent
    {
        [FieldOffset(0)]
        public MouseEvent Mouse;

        [FieldOffset(0)]
        public KeyboardEvent Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseEvent
    {
        public int X;
        public int Y;
        public int Data;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardEvent
    {
        public ushort KeyCode;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }
}
