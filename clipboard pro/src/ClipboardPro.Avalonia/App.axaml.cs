using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ClipboardPro.Avalonia.Services;
using ClipboardPro.Core.Data;
using ClipboardPro.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ClipboardPro.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Setup DB
            var dbPath = "clipboardpro.db";
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;
            
            var dbContext = new AppDbContext(options);
            dbContext.Database.EnsureCreated();

            // Setup Services
            var clipboardService = new AvaloniaClipboardService();
            var dispatcherService = new AvaloniaDispatcherService();

            // Setup ViewModel
            var viewModel = new MainViewModel(dbContext, clipboardService, dispatcherService);

            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}