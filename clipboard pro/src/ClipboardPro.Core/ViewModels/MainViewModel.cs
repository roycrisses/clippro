using System.Collections.ObjectModel;
using ClipboardPro.Core.Services;
using ClipboardPro.Data;
using ClipboardPro.Helpers;
using ClipboardPro.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace ClipboardPro.ViewModels;

/// <summary>
/// Main view model for the application
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly AppDbContext _dbContext;
    private readonly IClipboardService _clipboardService;
    private readonly IDispatcherService _dispatcherService;
    private CancellationTokenSource? _searchCts;

    [ObservableProperty]
    private ObservableCollection<Clipping> _clippings = new();

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    [ObservableProperty]
    private ObservableCollection<string> _sourceApps = new();

    [ObservableProperty]
    private Clipping? _selectedClipping;

    [ObservableProperty]
    private Project? _selectedProject;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedFilter = "All";

    [ObservableProperty]
    private Project? _lockedProject;

    [ObservableProperty]
    private bool _isMonitoringPaused;

    public MainViewModel(AppDbContext dbContext, IClipboardService clipboardService, IDispatcherService dispatcherService)
    {
        _dbContext = dbContext;
        _clipboardService = clipboardService;
        _dispatcherService = dispatcherService;
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        var clippings = await _dbContext.Clippings
            .OrderByDescending(c => c.Timestamp)
            .Take(500)
            .ToListAsync();
        
        Clippings = new ObservableCollection<Clipping>(clippings);

        var projects = await _dbContext.Projects.ToListAsync();
        Projects = new ObservableCollection<Project>(projects);

        var apps = await _dbContext.Clippings
            .Select(c => c.SourceApp)
            .Distinct()
            .ToListAsync();
        SourceApps = new ObservableCollection<string>(apps);

        LockedProject = await _dbContext.Projects.FirstOrDefaultAsync(p => p.IsLocked);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    partial void OnSearchQueryChanged(string value)
    {
        _ = ApplyFiltersAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        // Cancel any pending search
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            // Debounce to avoid rapid database/CPU operations
            await Task.Delay(300, token);

            var query = _dbContext.Clippings.AsQueryable();

            // Capture current filter values to avoid thread-safety issues
            string currentFilter = SelectedFilter;
            string currentSearch = SearchQuery;

            // Apply filter - extract values before using in LINQ
            if (currentFilter == "Favorites")
            {
                query = query.Where(c => c.IsFavorite);
            }
            else if (currentFilter.StartsWith("App:"))
            {
                var appName = currentFilter.Substring(4);
                query = query.Where(c => c.SourceApp == appName);
            }
            else if (currentFilter.StartsWith("Project:") && int.TryParse(currentFilter.Substring(8), out int projId))
            {
                query = query.Where(c => c.ProjectId == projId);
            }

            var clippings = await query
                .OrderByDescending(c => c.Timestamp)
                .Take(500)
                .ToListAsync(token);

            // Apply fuzzy search on a background thread if needed
            if (!string.IsNullOrWhiteSpace(currentSearch))
            {
                clippings = await Task.Run(() =>
                    FuzzySearch.Search(clippings, currentSearch, c => c.Content).ToList(),
                    token);
            }

            if (!token.IsCancellationRequested)
            {
                _dispatcherService.Invoke(() =>
                {
                    Clippings = new ObservableCollection<Clipping>(clippings);
                });
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CopyToClipboardAsync(Clipping clipping)
    {
        try
        {
            await _clipboardService.SetTextAsync(clipping.Content);
            
            // Update timestamp
            clipping.Timestamp = DateTime.Now;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Copy failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(Clipping clipping)
    {
        clipping.IsFavorite = !clipping.IsFavorite;
        await _dbContext.SaveChangesAsync();
    }

    [RelayCommand]
    private async Task DeleteClippingAsync(Clipping clipping)
    {
        _dbContext.Clippings.Remove(clipping);
        await _dbContext.SaveChangesAsync();
        Clippings.Remove(clipping);
    }

    [RelayCommand]
    private async Task CreateProjectAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var project = new Project { Name = name };
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();
        Projects.Add(project);
    }

    [RelayCommand]
    private async Task LockProjectAsync(Project? project)
    {
        // Unlock all projects first
        foreach (var p in await _dbContext.Projects.ToListAsync())
        {
            p.IsLocked = false;
        }

        // Lock selected project
        if (project != null)
        {
            project.IsLocked = true;
            LockedProject = project;
        }
        else
        {
            LockedProject = null;
        }

        await _dbContext.SaveChangesAsync();
    }

    [RelayCommand]
    private async Task AddToProjectAsync((Clipping clipping, Project project) args)
    {
        args.clipping.ProjectId = args.project.Id;
        await _dbContext.SaveChangesAsync();
    }

    [RelayCommand]
    private async Task DeleteProjectAsync(Project project)
    {
        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync();
        Projects.Remove(project);
        
        if (LockedProject?.Id == project.Id)
        {
            LockedProject = null;
        }
    }

    public async Task ClearAllClippingsAsync()
    {
        await _dbContext.Clippings.ExecuteDeleteAsync();
        Clippings.Clear();
        SourceApps.Clear();
    }

    public void OnClippingAdded(Clipping clipping)
    {
        _dispatcherService.Invoke(() =>
        {
            // Add to top of list
            Clippings.Insert(0, clipping);
            
            // Keep list manageable
            while (Clippings.Count > 500)
            {
                Clippings.RemoveAt(Clippings.Count - 1);
            }

            // Update source apps
            if (!SourceApps.Contains(clipping.SourceApp))
            {
                SourceApps.Add(clipping.SourceApp);
            }
        });
    }

    public void SetFilter(string filter)
    {
        SelectedFilter = filter;
        _ = ApplyFiltersAsync();
    }
}
