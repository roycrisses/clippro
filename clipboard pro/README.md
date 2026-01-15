# ClipBoard Pro

A high-performance, persistent Windows clipboard manager with modern Windows 11 aesthetics.

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4)
![Windows](https://img.shields.io/badge/Platform-Windows-0078D6)
![SQLite](https://img.shields.io/badge/Database-SQLite-003B57)

## Features

- **🔄 Real-time Clipboard Monitoring** - Captures text, hex codes, and links automatically
- **🔍 Instant Fuzzy Search** - Find any clip instantly with smart search
- **📁 Project Organization** - Lock projects to auto-tag new clips
- **⭐ Favorites** - Mark important clips for quick access
- **🎨 Source App Detection** - See which app each clip came from
- **🖥️ System Tray** - Runs quietly in background, always accessible
- **🔒 100% Local** - Zero network calls, all data stays on your machine

## Requirements

- Windows 10/11
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for building)
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (for running)

## Building from Source

### Quick Build

```powershell
cd "d:\zz\clipboard pro"
dotnet restore
dotnet build -c Release
```

### Create Standalone Executable

```powershell
# Self-contained EXE (includes .NET runtime, ~60MB)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Framework-dependent EXE (requires .NET 8 installed, ~5MB)
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

The executable will be in:
```
src\ClipboardPro\bin\Release\net8.0-windows\win-x64\publish\ClipboardPro.exe
```

## Usage

1. **Launch** the app - it starts monitoring immediately
2. **Copy** text from any application
3. **Click** any clip card to re-copy it
4. **Search** using the search bar for instant fuzzy matching
5. **Create Projects** to organize related clips
6. **Lock a Project** to auto-tag all new clips to it
7. **Close** the window - app minimizes to system tray

### System Tray Menu

Right-click the tray icon for:
- Open ClipBoard Pro
- Pause/Resume Monitoring
- Clear History
- Exit

## Data Location

Clipboard history is stored locally at:
```
%LOCALAPPDATA%\ClipboardPro\clipboard.db
```

## Architecture

```
ClipboardPro/
├── Models/          # Data models (Clipping, Project, Tag)
├── Data/            # EF Core DbContext and migrations
├── Services/        # ClipboardMonitor, SourceAppDetector
├── ViewModels/      # MVVM ViewModels
├── Views/           # XAML user controls
└── Helpers/         # NativeMethods, FuzzySearch
```

## Tech Stack

- **Framework**: WPF on .NET 8.0
- **UI**: WPF-UI (Fluent Design with Mica)
- **Database**: SQLite with EF Core
- **MVVM**: CommunityToolkit.Mvvm
- **Tray**: Hardcodet.NotifyIcon.Wpf

## Performance

- Idle RAM: < 50MB
- Instant search with fuzzy matching
- Virtualized list for large histories
- Duplicate detection to reduce storage

## License

MIT License - Free for personal and commercial use.
