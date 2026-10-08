using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Keymo;

/// <summary>
/// Sees every key press system-wide and lets the handler swallow it.
/// The handler gets the key with its Ctrl/Shift/Alt flags and returns true to consume it.
/// </summary>
internal sealed class KeyboardHook : IDisposable
{
    private const int LowLevelKeyboardHook = 13;
    private const int KeyDownMessage = 0x0100;
    private const int SystemKeyDownMessage = 0x0104;

    // A key code no keyboard has. Tapped while Alt is held so the focused app
    // does not open its menu bar when Alt comes up after a swallowed key.
    private const Keys AltMenuMaskKey = (Keys)0xE8;

    private readonly Func<Keys, bool> _onKeyDown;
    private readonly HookProc _callback; // Field keeps the delegate alive while Windows holds it.
    private readonly IntPtr _handle;

    public KeyboardHook(Func<Keys, bool> onKeyDown)
    {
        _onKeyDown = onKeyDown;
        _callback = Callback;
        _handle = SetWindowsHookEx(LowLevelKeyboardHook, _callback, GetModuleHandle(null), 0);
        if (_handle == IntPtr.Zero)
        {
            throw new Win32Exception();
        }
    }

    private delegate IntPtr HookProc(int code, IntPtr message, IntPtr data);

    /// <summary>While true every key passes through untouched.</summary>
    public bool Paused { get; set; }

    public void Dispose() => UnhookWindowsHookEx(_handle);

    private IntPtr Callback(int code, IntPtr message, IntPtr data)
    {
        bool isKeyDown = message == KeyDownMessage || message == SystemKeyDownMessage;
        if (code >= 0 && isKeyDown && !Paused && Swallows(data))
        {
            return 1;
        }

        return CallNextHookEx(_handle, code, message, data);
    }

    private bool Swallows(IntPtr data)
    {
        var key = (Keys)Marshal.ReadInt32(data);
        if (key == AltMenuMaskKey || IsModifier(key))
        {
            return false;
        }

        Keys modifiers = Held(Keys.ControlKey, Keys.Control) | Held(Keys.ShiftKey, Keys.Shift) | Held(Keys.Menu, Keys.Alt);
        if (!_onKeyDown(key | modifiers))
        {
            return false;
        }

        if (modifiers.HasFlag(Keys.Alt))
        {
            Input.TapKey(AltMenuMaskKey);
        }

        return true;
    }

    private static bool IsModifier(Keys key) =>
        key is Keys.ControlKey or Keys.LControlKey or Keys.RControlKey
            or Keys.ShiftKey or Keys.LShiftKey or Keys.RShiftKey
            or Keys.Menu or Keys.LMenu or Keys.RMenu
            or Keys.LWin or Keys.RWin;

    private static Keys Held(Keys key, Keys flag) => GetAsyncKeyState((int)key) < 0 ? flag : Keys.None;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int hookId, HookProc callback, IntPtr module, uint threadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hook);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr data);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int key);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? moduleName);
}
