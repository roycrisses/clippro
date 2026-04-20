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

    private CancellationTokenSource? _searchCts;

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
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                // 300ms debounce to prevent excessive DB queries and CPU usage during rapid typing
                await Task.Delay(300, token);
                await ApplyFiltersAsync(token);
            }
            catch (OperationCanceledException)
            {
                // Search was cancelled by a newer one, safe to ignore
            }
        }, token);
    }

    private async Task ApplyFiltersAsync(CancellationToken ct)
    {
        try
        {
            var query = _dbContext.Clippings.AsQueryable();

            // Apply filter - extract values before using in LINQ
            if (SelectedFilter == "Favorites")
            {
                query = query.Where(c => c.IsFavorite);
            }
            else if (SelectedFilter.StartsWith("App:"))
            {
                var appName = SelectedFilter.Substring(4);
                query = query.Where(c => c.SourceApp == appName);
            }
            else if (SelectedFilter.StartsWith("Project:") && int.TryParse(SelectedFilter.Substring(8), out int projId))
            {
                query = query.Where(c => c.ProjectId == projId);
            }

            // Execute DB query with cancellation support
            var clippings = await query
                .OrderByDescending(c => c.Timestamp)
                .Take(500)
                .ToListAsync(ct);

            // Apply fuzzy search (CPU intensive)
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                clippings = FuzzySearch.Search(clippings, SearchQuery, c => c.Content).ToList();
            }

            // Check cancellation before updating UI
            ct.ThrowIfCancellationRequested();

            _dispatcherService.Invoke(() =>
            {
                Clippings = new ObservableCollection<Clipping>(clippings);
            });
        }
        catch (OperationCanceledException)
        {
            // Search was cancelled, safe to ignore
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
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = new CancellationTokenSource();
        _ = ApplyFiltersAsync(_searchCts.Token);
    }
}
