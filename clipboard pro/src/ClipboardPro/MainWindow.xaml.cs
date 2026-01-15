using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ClipboardPro.Models;
using ClipboardPro.Services;
using ClipboardPro.ViewModels;
using Wpf.Ui.Controls;

namespace ClipboardPro;

public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _viewModel;
    private ClipboardMonitor? _clipboardMonitor;
    private HotkeyManager? _hotkeyManager;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = App.MainViewModel!;
        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;
        UpdateStats();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize clipboard monitor
        _clipboardMonitor = new ClipboardMonitor(this, App.DbContext!);
        _clipboardMonitor.ClippingAdded += OnClippingAdded;

        // Initialize global hotkey (Win + Shift + C)
        _hotkeyManager = new HotkeyManager(this, ShowWindow);
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        Topmost = true;  // Bring to front
        Topmost = false; // Allow other windows on top later
    }

    private void OnClippingAdded(object? sender, Clipping clipping)
    {
        _viewModel.OnClippingAdded(clipping);
        Dispatcher.Invoke(UpdateStats);
    }

    private void UpdateStats()
    {
        StatsText.Text = $"{_viewModel.Clippings.Count} clips";
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Minimize to tray instead of closing
        e.Cancel = true;
        Hide();
    }

    // Filter handlers
    private void FilterAll_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SetFilter("All");
    }

    private void FilterFavorites_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SetFilter("Favorites");
    }

    private void FilterProject_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.Button button && button.Tag is int projectId)
        {
            _viewModel.SetFilter($"Project:{projectId}");
        }
    }

    private void FilterApp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.Button button && button.Tag is string appName)
        {
            _viewModel.SetFilter($"App:{appName}");
        }
    }

    // Clipping actions
    private async void CopyClipping_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.Button button && button.Tag is Clipping clipping)
        {
            await _viewModel.CopyToClipboardCommand.ExecuteAsync(clipping);
        }
    }

    private async void ToggleFavorite_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.Button button && button.Tag is Clipping clipping)
        {
            await _viewModel.ToggleFavoriteCommand.ExecuteAsync(clipping);
        }
    }

    private async void DeleteClipping_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.Button button && button.Tag is Clipping clipping)
        {
            await _viewModel.DeleteClippingCommand.ExecuteAsync(clipping);
            UpdateStats();
        }
    }

    // Project actions
    private async void AddProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Create New Project",
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel"
        };

        var textBox = new Wpf.Ui.Controls.TextBox
        {
            PlaceholderText = "Project name..."
        };
        dialog.Content = textBox;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            await _viewModel.CreateProjectCommand.ExecuteAsync(textBox.Text);
        }
    }

    private async void UnlockProject_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LockProjectCommand.ExecuteAsync(null);
    }
}

// Value Converters
public class ClippingTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            ClippingType.HexCode => "🎨",
            ClippingType.Link => "🔗",
            _ => "📝"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class TimeAgoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime timestamp)
        {
            var diff = DateTime.Now - timestamp;
            return diff.TotalMinutes switch
            {
                < 1 => "just now",
                < 60 => $"{(int)diff.TotalMinutes}m ago",
                < 1440 => $"{(int)diff.TotalHours}h ago",
                _ => $"{(int)diff.TotalDays}d ago"
            };
        }
        return "unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter as string == "star")
        {
            return value is true ? "⭐" : "☆";
        }
        
        return value switch
        {
            bool b => b ? Visibility.Visible : Visibility.Collapsed,
            null => Visibility.Collapsed,
            _ => Visibility.Visible
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
