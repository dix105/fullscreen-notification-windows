# Fullscreen Notification Overlay

A small Windows desktop app that mirrors Discord toast notifications as a bottom-right always-on-top overlay while another app is fullscreen.

## What it does

- Requests Windows notification-listener access.
- Watches new Windows toast notifications.
- Filters to Discord notifications only.
- Detects if the foreground window is fullscreen.
- Shows a Discord-like bottom-right banner overlay with a close button above fullscreen/borderless apps.

## Important limitation

Windows can block normal desktop overlays above true exclusive-fullscreen DirectX games. For games/video apps, use **Borderless Fullscreen** for reliable overlays. The close button needs the overlay to be clickable, so it is not fully click-through.

## Build on Windows

Requirements:

- Windows 10/11
- .NET 8 SDK

```powershell
cd src\FullscreenNotificationOverlay
dotnet restore
dotnet run
```

To publish a single EXE:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

The EXE will be in:

```text
bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\FullscreenNotificationOverlay.exe
```
