using System.Runtime.InteropServices;

namespace Keymo;

/// <summary>Injects mouse wheel, mouse button and key events as if the user made them.</summary>
internal static class Input
{
    private const int WheelNotch = 120;
    private const uint MouseType = 0;
    private const uint KeyboardType = 1;
    private const uint LeftDown = 0x0002;
    private const uint LeftUp = 0x0004;
    private const uint RightDown = 0x0008;
    private const uint RightUp = 0x0010;
    private const uint VerticalWheel = 0x0800;
    private const uint HorizontalWheel = 0x1000;
    private const uint KeyUp = 0x0002;

    /// <summary>Scrolls by wheel notches. Positive is right and up.</summary>
    public static void Scroll(int rightNotches, int upNotches)
    {
        if (rightNotches != 0)
        {
            Send(Mouse(HorizontalWheel, rightNotches * WheelNotch));
        }

        if (upNotches != 0)
        {
            Send(Mouse(VerticalWheel, upNotches * WheelNotch));
        }
    }

    public static void Click(bool rightButton)
    {
        if (rightButton)
        {
            Send(Mouse(RightDown), Mouse(RightUp));
        }
        else
        {
            Send(Mouse(LeftDown), Mouse(LeftUp));
        }
    }

    public static void TapKey(Keys key) => Send(Key(key, 0), Key(key, KeyUp));

    private static NativeInput Mouse(uint flags, int data = 0) =>
        new() { Type = MouseType, Event = { Mouse = { Flags = flags, Data = data } } };

    private static NativeInput Key(Keys key, uint flags) =>
        new() { Type = KeyboardType, Event = { Keyboard = { KeyCode = (ushort)key, Flags = flags } } };

    // A failed injection only loses one click or scroll step, so the result is not checked.
    private static void Send(params NativeInput[] inputs) =>
        _ = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeInput>());

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
