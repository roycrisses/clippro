using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using ClipboardPro.Data;
using ClipboardPro.Helpers;
using ClipboardPro.Models;
using Microsoft.EntityFrameworkCore;

namespace ClipboardPro.Services;

/// <summary>
/// Monitors clipboard changes and stores clippings to the database
/// </summary>
public class ClipboardMonitor : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly HwndSource _hwndSource;
    private string _lastContentHash = string.Empty;
    private DateTime _lastCopyTime = DateTime.MinValue;
    private bool _disposed;

    /// <summary>
    /// When true, clipboard monitoring is paused
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// Event raised when a new clipping is added
    /// </summary>
    public event EventHandler<Clipping>? ClippingAdded;

    public ClipboardMonitor(Window window, AppDbContext dbContext)
    {
        _dbContext = dbContext;
        
        var helper = new WindowInteropHelper(window);
        _hwndSource = HwndSource.FromHwnd(helper.Handle) 
            ?? throw new InvalidOperationException("Failed to get HwndSource");
        
        _hwndSource.AddHook(WndProc);
        
        if (!NativeMethods.AddClipboardFormatListener(helper.Handle))
        {
            throw new InvalidOperationException("Failed to add clipboard format listener");
        }

        App.RegisterClipboardMonitor(this);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_CLIPBOARDUPDATE && !IsPaused)
        {
            Task.Run(ProcessClipboardChange);
        }
        return IntPtr.Zero;
    }

    private async Task ProcessClipboardChange()
    {
        try
        {
            string? content = null;

            // Try to get clipboard content with retry logic for "clipboard busy" errors
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (Clipboard.ContainsText())
                        {
                            content = Clipboard.GetText();
                        }
                    });
                    break;
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    // Clipboard is busy, wait and retry
                    await Task.Delay(100 * (retry + 1));
                }
            }

            if (string.IsNullOrWhiteSpace(content))
                return;

            // Check for duplicates (same content within 500ms)
            var contentHash = ComputeHash(content);
            var now = DateTime.Now;

            if (contentHash == _lastContentHash && 
                (now - _lastCopyTime).TotalMilliseconds < 500)
            {
                return; // Skip duplicate
            }

            _lastContentHash = contentHash;
            _lastCopyTime = now;

            // Check if this exact content already exists in database
            var existingClipping = await _dbContext.Clippings
                .FirstOrDefaultAsync(c => c.ContentHash == contentHash);

            if (existingClipping != null)
            {
                // Update timestamp of existing clipping instead of creating duplicate
                existingClipping.Timestamp = now;
                existingClipping.SourceApp = SourceAppDetector.GetCurrentAppName();
                await _dbContext.SaveChangesAsync();
                ClippingAdded?.Invoke(this, existingClipping);
                return;
            }

            // Create new clipping
            var clipping = new Clipping
            {
                Content = content,
                Preview = content.Length > 200 ? content[..200] + "..." : content,
                Type = DetectContentType(content),
                SourceApp = SourceAppDetector.GetCurrentAppName(),
                Timestamp = now,
                ContentHash = contentHash
            };

            // Auto-assign to locked project if any
            var lockedProject = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.IsLocked);
            
            if (lockedProject != null)
            {
                clipping.ProjectId = lockedProject.Id;
            }

            _dbContext.Clippings.Add(clipping);
            await _dbContext.SaveChangesAsync();

            ClippingAdded?.Invoke(this, clipping);
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Clipboard processing error: {ex.Message}");
        }
    }

    private static ClippingType DetectContentType(string content)
    {
        content = content.Trim();

        // Check for hex color codes
        if (Regex.IsMatch(content, @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$"))
        {
            return ClippingType.HexCode;
        }

        // Check for URLs
        if (Uri.TryCreate(content, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return ClippingType.Link;
        }

        // Check for common URL patterns without scheme
        if (Regex.IsMatch(content, @"^(www\.)?[\w-]+\.[\w.-]+(/\S*)?$", RegexOptions.IgnoreCase))
        {
            return ClippingType.Link;
        }

        return ClippingType.Text;
    }

    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_hwndSource != null)
        {
            var hwnd = _hwndSource.Handle;
            NativeMethods.RemoveClipboardFormatListener(hwnd);
            _hwndSource.RemoveHook(WndProc);
        }

        GC.SuppressFinalize(this);
    }
}
