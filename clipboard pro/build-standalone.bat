@echo off
echo ============================================
echo ClipBoard Pro - Self-Contained Build
echo ============================================
echo This creates a standalone EXE that includes
echo the .NET runtime (~60MB total size)
echo ============================================
echo.

where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 8.0 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo [1/3] Restoring packages...
dotnet restore

echo.
echo [2/3] Publishing self-contained executable...
dotnet publish src\ClipboardPro\ClipboardPro.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o .\publish-standalone

echo.
echo ============================================
echo BUILD COMPLETE!
echo ============================================
echo.
echo Standalone executable: publish-standalone\ClipboardPro.exe
echo This EXE can run on any Windows machine without .NET installed.
echo.
pause
