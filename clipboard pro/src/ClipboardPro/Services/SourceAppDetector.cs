using System.Diagnostics;
using System.Text;
using ClipboardPro.Helpers;

namespace ClipboardPro.Services;

/// <summary>
/// Detects the source application that initiated a clipboard operation
/// </summary>
public static class SourceAppDetector
{
    /// <summary>
    /// Gets the name of the application that currently has focus
    /// </summary>
    public static string GetCurrentAppName()
    {
        try
        {
            var hwnd = NativeMethods.GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                return "Unknown";

            NativeMethods.GetWindowThreadProcessId(hwnd, out uint processId);

            if (processId == 0)
                return "Unknown";

            var process = Process.GetProcessById((int)processId);
            return GetFriendlyAppName(process.ProcessName);
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Gets a user-friendly name for common applications
    /// </summary>
    private static string GetFriendlyAppName(string processName)
    {
        return processName.ToLowerInvariant() switch
        {
            "chrome" => "Chrome",
            "firefox" => "Firefox",
            "msedge" => "Edge",
            "code" => "VS Code",
            "devenv" => "Visual Studio",
            "figma" => "Figma",
            "notepad" => "Notepad",
            "notepad++" => "Notepad++",
            "explorer" => "Explorer",
            "outlook" => "Outlook",
            "teams" => "Teams",
            "slack" => "Slack",
            "discord" => "Discord",
            "excel" => "Excel",
            "winword" => "Word",
            "powerpnt" => "PowerPoint",
            "windowsterminal" => "Terminal",
            "cmd" => "Command Prompt",
            "powershell" => "PowerShell",
            "pwsh" => "PowerShell",
            _ => CapitalizeFirstLetter(processName)
        };
    }

    private static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input[1..];
    }

    /// <summary>
    /// Gets the window title of the foreground window
    /// </summary>
    public static string GetCurrentWindowTitle()
    {
        try
        {
            var hwnd = NativeMethods.GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                return string.Empty;

            int length = NativeMethods.GetWindowTextLength(hwnd);
            if (length == 0)
                return string.Empty;

            var sb = new StringBuilder(length + 1);
            NativeMethods.GetWindowText(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }
}
