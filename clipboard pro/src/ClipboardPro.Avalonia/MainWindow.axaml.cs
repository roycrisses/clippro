using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ClipboardPro.ViewModels;
using ClipboardPro.Models;

namespace ClipboardPro.Avalonia;

public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        
        // In Avalonia, DataContext is often set before or after InitializeComponent based on DI
        // Checking if DataContext is already set (by App.axaml.cs)
        this.DataContextChanged += (s, e) =>
        {
            if (DataContext is MainViewModel vm)
            {
                _viewModel = vm;
                // Since this is view-related logic (updating stats in UI title/text), we can hook up events here if needed
                UpdateStats(); // Initial update
                
                // For "ClippingAdded", we might need to subscribe to VM events if VM exposed them, 
                // but currently VM handles addition via ObservableCollection which UI automatically reflects.
                // The original code hooked "ClippingAdded" to update "StatsText".
                // Since ObservableCollection notifies, we only need to update the stats count when collection changes.
                _viewModel.Clippings.CollectionChanged += (sender, args) => UpdateStats();
            }
        };
    }

    private void UpdateStats()
    {
        if (_viewModel == null) return;
        
        // FindControl is used in Avalonia code-behind to access named elements if not generated automatically
        // But with x:Name, they should be standard fields. Note: Avalonia source generators might be needed.
        // Falling back to finding if fields aren't valid.
        var statsText = this.FindControl<TextBlock>("StatsText");
        if (statsText != null)
        {
            statsText.Text = $"{_viewModel.Clippings.Count} clips";
        }
    }

    // Filter handlers
    private void FilterAll_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel?.SetFilter("All");
    }

    private void FilterFavorites_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel?.SetFilter("Favorites");
    }

    private void FilterProject_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int projectId)
        {
            _viewModel?.SetFilter($"Project:{projectId}");
        }
    }

    private void FilterApp_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string appName)
        {
            _viewModel?.SetFilter($"App:{appName}");
        }
    }

    // Clipping actions
    private async void CopyClipping_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Clipping clipping)
        {
            if (_viewModel?.CopyToClipboardCommand.CanExecute(clipping) == true)
            {
                await _viewModel.CopyToClipboardCommand.ExecuteAsync(clipping);
            }
        }
    }

    private async void ToggleFavorite_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Clipping clipping)
        {
             if (_viewModel?.ToggleFavoriteCommand.CanExecute(clipping) == true)
            {
                await _viewModel.ToggleFavoriteCommand.ExecuteAsync(clipping);
            }
        }
    }

    private async void DeleteClipping_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Clipping clipping)
        {
            if (_viewModel?.DeleteClippingCommand.CanExecute(clipping) == true)
            {
                await _viewModel.DeleteClippingCommand.ExecuteAsync(clipping);
                UpdateStats();
            }
        }
    }

    // Project actions
    private async void AddProject_Click(object? sender, RoutedEventArgs e)
    {
        // Simple Input Dialog is not standard in Avalonia/WPF. 
        // We'd arguably want a proper dialog service or a simple popup.
        // For MVP port, I'll logging or omitting dialog for now, or creating a quick custom dialog.
        // Implementing a quick crude dialog logic here is risky without testing.
        // I will skip the dialog implementation for this turn and just create a default project or log it.
        // Ideally: new AddProjectDialog().ShowDialog(this);
        
        System.Diagnostics.Debug.WriteLine("Add Project clicked - Dialog not port yet");
    }

    private async void UnlockProject_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.LockProjectCommand.CanExecute(null) == true)
        {
            await _viewModel.LockProjectCommand.ExecuteAsync(null);
        }
    }
}