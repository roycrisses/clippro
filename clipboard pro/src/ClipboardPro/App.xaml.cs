using System.IO;
using System.Windows;
using ClipboardPro.Data;
using ClipboardPro.Services;
using ClipboardPro.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.EntityFrameworkCore;

namespace ClipboardPro;

public partial class App : Application
{
    private TaskbarIcon? _notifyIcon;
    private ClipboardMonitor? _clipboardMonitor;
    private AppDbContext? _dbContext;
    
    public static MainViewModel? MainViewModel { get; private set; }
    public static AppDbContext? DbContext { get; private set; }
    public static ClipboardMonitor? ClipboardMonitor { get; private set; }
    private bool _startMinimized;

    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
        "ClipboardPro_Log.txt");

    public App()
    {
        Log("=== Application Starting ===");
        Log($"Time: {DateTime.Now}");
        Log($"OS: {Environment.OSVersion}");
        Log($".NET Version: {Environment.Version}");
        Log($"Process Path: {Environment.ProcessPath}");
        Log($"Current Directory: {Environment.CurrentDirectory}");
        
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Log("Exception handlers registered");
    }

    private static void Log(string message)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n");
        }
        catch { /* Ignore logging failures */ }
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log($"DISPATCHER EXCEPTION: {e.Exception}");
        LogError(e.Exception, "DispatcherUnhandledException");
        e.Handled = true; // Prevent crash if possible
        Shutdown(1);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log($"APPDOMAIN EXCEPTION: {e.ExceptionObject}");
        LogError(e.ExceptionObject as Exception, "AppDomain.UnhandledException");
    }

    private void LogError(Exception? ex, string source)
    {
        if (ex == null) return;

        try
        {
            var logContent = $"[{DateTime.Now}] [{source}] Critical Error:\n{ex}\n\nInner: {ex.InnerException}\nStack: {ex.StackTrace}\n\n";
            
            // Try AppData first
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClipboardPro", "crash.log");
            var dir = Path.GetDirectoryName(appDataPath);
            if (!Directory.Exists(dir) && dir != null) Directory.CreateDirectory(dir);
            File.AppendAllText(appDataPath, logContent);

            // Also try Desktop for visibility
            var desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ClipboardPro_Crash.txt");
            File.AppendAllText(desktopPath, logContent);

            MessageBox.Show($"Clipboard Pro encountered a critical error.\n\nLog saved to:\n{desktopPath}\n\nError: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception logEx)
        {
            MessageBox.Show($"Fatal Error: {ex.Message}\n\nFailed to write log: {logEx.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Log("OnStartup() called");
        base.OnStartup(e);

        // Check if started with --minimized argument (from Windows Startup)
        _startMinimized = e.Args.Contains("--minimized");
        Log($"Start minimized: {_startMinimized}");

        try
        {
            // Initialize settings
            Log("Loading settings...");
            SettingsService.Load();
            Log("Settings loaded");

            // Initialize database
            Log("Initializing database...");
            InitializeDatabase();
            Log("Database initialized");

            // Initialize ViewModel
            Log("Creating MainViewModel...");
            MainViewModel = new MainViewModel(_dbContext!);
            Log("MainViewModel created");

            // Enable startup on first run
            Log("Checking startup registration...");
            if (!StartupManager.IsStartupEnabled())
            {
                StartupManager.SetStartupEnabled(true);
                Log("Startup enabled");
            }

            // Initialize system tray icon
            Log("Initializing notify icon...");
            InitializeNotifyIcon();
            Log("Notify icon initialized");

            // Show onboarding if first run
            Log($"IsFirstRun: {SettingsService.Settings.IsFirstRun}");
            if (SettingsService.Settings.IsFirstRun)
            {
                Log("Showing onboarding window...");
                var onboarding = new Views.OnboardingWindow();
                onboarding.ShowDialog();
                Log("Onboarding closed");
                
                SettingsService.Settings.IsFirstRun = false;
                SettingsService.Save();
            }

            // Create main window (hidden if started minimized)
            MainWindow = new MainWindow();
            
            if (!_startMinimized)
            {
                MainWindow.Show();
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "OnStartup");
            Shutdown(1);
        }
    }

    private void InitializeDatabase()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClipboardPro",
            "clipboard.db");

        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _dbContext = new AppDbContext(dbPath);
        _dbContext.Database.Migrate();
        DbContext = _dbContext;
    }

    private void InitializeNotifyIcon()
    {
        System.Drawing.Icon? icon = null;
        
        try
        {
            var iconStream = Application.GetResourceStream(
                new Uri("pack://application:,,,/Assets/icon.ico"))?.Stream;
            if (iconStream != null)
            {
                icon = new System.Drawing.Icon(iconStream);
            }
        }
        catch
        {
            // Icon not found, use system default
        }

        // Use system default icon if custom icon not available
        icon ??= System.Drawing.SystemIcons.Application;

        _notifyIcon = new TaskbarIcon
        {
            Icon = icon,
            ToolTipText = "ClipBoard Pro - Click to open",
            ContextMenu = CreateContextMenu(),
            DoubleClickCommand = new RelayCommand(ShowMainWindow)
        };
    }

    private System.Windows.Controls.ContextMenu CreateContextMenu()
    {
        var menu = new System.Windows.Controls.ContextMenu();
        
        var openItem = new System.Windows.Controls.MenuItem { Header = "Open ClipBoard Pro" };
        openItem.Click += (s, e) => ShowMainWindow();
        menu.Items.Add(openItem);

        var pauseItem = new System.Windows.Controls.MenuItem { Header = "Pause Monitoring" };
        pauseItem.Click += (s, e) =>
        {
            if (ClipboardMonitor != null)
            {
                ClipboardMonitor.IsPaused = !ClipboardMonitor.IsPaused;
                pauseItem.Header = ClipboardMonitor.IsPaused ? "Resume Monitoring" : "Pause Monitoring";
            }
        };
        menu.Items.Add(pauseItem);

        menu.Items.Add(new System.Windows.Controls.Separator());

        var clearItem = new System.Windows.Controls.MenuItem { Header = "Clear History" };
        clearItem.Click += async (s, e) =>
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all clipboard history?",
                "Clear History",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes && MainViewModel != null)
            {
                await MainViewModel.ClearAllClippingsAsync();
            }
        };
        menu.Items.Add(clearItem);

        menu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
        exitItem.Click += (s, e) =>
        {
            _notifyIcon?.Dispose();
            Application.Current.Shutdown();
        };
        menu.Items.Add(exitItem);

        return menu;
    }

    private void ShowMainWindow()
    {
        if (MainWindow == null)
        {
            MainWindow = new MainWindow();
        }

        MainWindow.Show();
        MainWindow.WindowState = WindowState.Normal;
        MainWindow.Activate();
        MainWindow.Topmost = true;  // Force to front
        MainWindow.Topmost = false; // Allow other windows later
    }

    public static void RegisterClipboardMonitor(ClipboardMonitor monitor)
    {
        ClipboardMonitor = monitor;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _notifyIcon?.Dispose();
        _clipboardMonitor?.Dispose();
        _dbContext?.Dispose();
        base.OnExit(e);
    }
}

public class RelayCommand : System.Windows.Input.ICommand
{
    private readonly Action _execute;
    
    public RelayCommand(Action execute) => _execute = execute;
    
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged;
}
