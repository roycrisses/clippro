using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ClipboardPro.Core.Services;

namespace ClipboardPro.Avalonia.Services;

public class AvaloniaClipboardService : IClipboardService
{
    private Avalonia.Input.IClipboard? GetClipboard()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.Clipboard;
        }
        return null; // Handle other lifetimes if needed
    }

    public async Task SetTextAsync(string text)
    {
        var clipboard = GetClipboard();
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(text);
        }
    }

    public async Task<string?> GetTextAsync()
    {
        var clipboard = GetClipboard();
        if (clipboard != null)
        {
            return await clipboard.GetTextAsync();
        }
        return null;
    }
}
