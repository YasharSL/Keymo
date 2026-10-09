using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Keymo;

/// <summary>
/// Sees every key press and release system-wide and lets the handler swallow it.
/// The handler gets the key with its Ctrl/Shift/Alt flags and whether it went down, and returns true to consume it.
/// </summary>
internal sealed class KeyboardHook : IDisposable
{
    private const int LowLevelKeyboardHook = 13;
    private const int KeyDownMessage = 0x0100;
    private const int SystemKeyDownMessage = 0x0104;
    private const int FlagsOffset = 8;
    private const int InjectedFlag = 0x10;

    private readonly Func<Keys, bool, bool> _onKey;
    private readonly HookProc _callback; // Field keeps the delegate alive while Windows holds it.
    private readonly IntPtr _handle;

    // True while the user still holds Alt but an injected key-up (from Input.Scroll) has lifted it
    // as far as Windows is concerned, so the system key state no longer reports it.
    private bool _altHeldButLifted;

    public KeyboardHook(Func<Keys, bool, bool> onKey)
    {
        _onKey = onKey;
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
        if (code >= 0)
        {
            bool isKeyDown = message == KeyDownMessage || message == SystemKeyDownMessage;
            var key = (Keys)Marshal.ReadInt32(data);
            bool injected = (Marshal.ReadInt32(data, FlagsOffset) & InjectedFlag) != 0;
            if (key is Keys.Menu or Keys.LMenu or Keys.RMenu)
            {
                // Any real Alt press or release, or an injected press, ends the lifted state.
                _altHeldButLifted = injected && !isKeyDown;
            }

            if (!Paused && Swallows(key, isKeyDown))
            {
                return 1;
            }
        }

        return CallNextHookEx(_handle, code, message, data);
    }

    private bool Swallows(Keys key, bool isKeyDown)
    {
        if (key == Input.AltMenuMaskKey || IsModifier(key))
        {
            return false;
        }

        Keys alt = _altHeldButLifted ? Keys.Alt : Held(Keys.Menu, Keys.Alt);
        Keys modifiers = Held(Keys.ControlKey, Keys.Control) | Held(Keys.ShiftKey, Keys.Shift) | alt;
        if (!_onKey(key | modifiers, isKeyDown))
        {
            return false;
        }

        // Without this the focused app opens its menu bar when Alt comes up after a swallowed key.
        if (isKeyDown && modifiers.HasFlag(Keys.Alt))
        {
            Input.TapKey(Input.AltMenuMaskKey);
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
