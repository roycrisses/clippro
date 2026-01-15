@echo off
echo ============================================
echo ClipBoard Pro Build Script
echo ============================================
echo.

REM Check for .NET SDK
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 8.0 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo [1/4] Restoring packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Package restore failed!
    pause
    exit /b 1
)

echo.
echo [2/4] Building solution...
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo.
echo [3/4] Publishing standalone executable...
dotnet publish src\ClipboardPro\ClipboardPro.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o .\publish

echo.
echo ============================================
echo BUILD COMPLETE!
echo ============================================
echo.
echo Executable location: publish\ClipboardPro.exe
echo.
pause
