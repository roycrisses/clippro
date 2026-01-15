using System.Windows;
using System.Windows.Interop;
using ClipboardPro.Helpers;

namespace ClipboardPro.Services;

/// <summary>
/// Manages global hotkey registration for the application
/// </summary>
public class HotkeyManager : IDisposable
{
    private const int HOTKEY_ID = 9000;
    private readonly Window _window;
    private readonly HwndSource _hwndSource;
    private readonly Action _onHotkeyPressed;
    private bool _disposed;

    // Win + Shift + C
    private const uint VK_C = 0x43;

    public HotkeyManager(Window window, Action onHotkeyPressed)
    {
        _window = window;
        _onHotkeyPressed = onHotkeyPressed;

        var helper = new WindowInteropHelper(window);
        _hwndSource = HwndSource.FromHwnd(helper.Handle)
            ?? throw new InvalidOperationException("Failed to get HwndSource for hotkey");

        _hwndSource.AddHook(WndProc);

        // Register Win + Shift + C
        var success = NativeMethods.RegisterHotKey(
            helper.Handle,
            HOTKEY_ID,
            NativeMethods.MOD_WIN | NativeMethods.MOD_SHIFT | NativeMethods.MOD_NOREPEAT,
            VK_C);

        if (!success)
        {
            System.Diagnostics.Debug.WriteLine("Failed to register hotkey Win+Shift+C");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            _onHotkeyPressed?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        var helper = new WindowInteropHelper(_window);
        NativeMethods.UnregisterHotKey(helper.Handle, HOTKEY_ID);
        _hwndSource?.RemoveHook(WndProc);

        GC.SuppressFinalize(this);
    }
}
