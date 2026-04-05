using Microsoft.Win32;

namespace ClipboardPro.Services;

/// <summary>
/// Manages Windows startup registration
/// </summary>
public static class StartupManager
{
    private const string APP_NAME = "ClipboardPro";
    private const string REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Checks if app is set to run on startup
    /// </summary>
    public static bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false);
            return key?.GetValue(APP_NAME) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Enables or disables startup with Windows
    /// </summary>
    public static bool SetStartupEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
            if (key == null) return false;

            if (enabled)
            {
                // Get the current executable path
                var exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath)) return false;

                // Add --minimized argument for silent startup
                key.SetValue(APP_NAME, $"\"{exePath}\" --minimized");
            }
            else
            {
                key.DeleteValue(APP_NAME, false);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
