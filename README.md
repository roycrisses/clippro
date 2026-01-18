# ClipBoard Pro

![Banner](clipboard%20pro/website/assets/icon.png)

> **The Cinematic, Cross-Platform Clipboard Manager for Power Users.**

![Windows](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D6?style=for-the-badge&logo=windows)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Public%20Beta-orange?style=for-the-badge)

## Public Beta Notice

Clipboard Pro is currently in **Public Beta**. We are actively improving performance and adding features.
> **Found a bug?** Post a screenshot on X and tag **[@m_Krishnakarki](https://x.com/m_Krishnakarki)**.

---

## Architecture & Workflow

The following diagram illustrates how Clipboard Pro captures, processes, and displays your data securely on your local machine.

```mermaid
graph TD
    subgraph "System Inputs"
        User[User Actions]
        SysClip[System Clipboard]
        GlobalKeys[Global Hotkeys]
    end

    subgraph "Background Services"
        Monitor[Clipboard Monitor]
        Detector[Source App Detector]
        HotKeySvc[Hotkey Service]
    end

    subgraph "Core Logic (ViewModel)"
        MVM[MainViewModel]
        Filter[Filter Engine]
        Fuzzy[Fuzzy Search]
    end

    subgraph "Data Persistence"
        EF[Entity Framework Core]
        DB[(SQLite Database)]
    end

    subgraph "User Interface"
        UI[WPF Fluent Window]
        Tray[System Tray Icon]
    end

    %% Flow Connections
    User -->|Ctrl+C| SysClip
    SysClip -->|WM_CLIPBOARDUPDATE| Monitor
    Monitor -->|Get Foreground Process| Detector
    Detector -->|App Name + Icon| Monitor
    Monitor -->|New Clipping| MVM

    GlobalKeys -->|Win+Shift+C| HotKeySvc
    HotKeySvc -->|Toggle Visibility| UI

    MVM -->|Save Async| EF
    EF -->|Write| DB
    DB -->|Read History| EF
    EF -->|Load ObservableCollection| MVM

    MVM -->|Query| Filter
    MVM -->|Search Text| Fuzzy
    Fuzzy -->|Ranked Results| UI
    Filter -->|Filtered List| UI

    Tray -->|Right Click| HotKeySvc
    User -->|Interact| UI
    UI -->|Paste Command| SysClip
    
    style DB fill:#1e1e1e,stroke:#FF7A00,stroke-width:2px,color:#fff
    style UI fill:#2d2d2d,stroke:#00A3FF,stroke-width:2px,color:#fff
    style MVM fill:#2d2d2d,stroke:#fff,stroke-dasharray: 5 5,color:#fff
```

## Features

- **Cinematic UI**: A "Glassmorphism" design that feels premium and native to Windows 11.
- **Instant Fuzzy Search**: Find any clip in milliseconds, even with typos.
- **Smart History**: Automatically captures text, links, and code snippets.
- **Project Workflows**: Organize clips into "Projects" to keep your work context-focused.
- **100% Private**: Your data is stored locally in an encrypted SQLite database. Zero cloud uploads.
- **Power Shortcuts**: 
    - `Win + Shift + C` : Open Clipboard Pro instantly.
    - `Enter` : Paste selected clip.

## Installation

### Option 1: Download (Recommended)
Visit our [Official Website](https://clippro.netlify.app/) to download the latest Windows installer.

> **Note on SmartScreen**: Since this is an open-source beta project, Windows SmartScreen may flag the installer. Click **"More Info" -> "Run Anyway"** to install. Providing a code-signing certificate is on our roadmap!

### Option 2: Build from Source
Requirements: [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

```powershell
# Clone the repository
git clone https://github.com/roycrisses/clippro.git
cd clippro

# Build and Run
dotnet build -c Release
dotnet run --project src/ClipboardPro/ClipboardPro.csproj
```

## Tech Stack

- **Core**: .NET 8.0 (C#)
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Styling**: [WPF-UI](https://wpfui.lepo.co/) (Fluent Design System)
- **Database**: SQLite + Entity Framework Core
- **Architecture**: MVVM (Model-View-ViewModel) with CommunityToolkit

## Contributing

We welcome contributions!
1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Author

**Krishna Karki**
- X (Twitter): [@m_Krishnakarki](https://x.com/m_Krishnakarki)

## License

Distributed under the MIT License. See `LICENSE` for more information.
