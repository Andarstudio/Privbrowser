# PrivBrowser

A privacy-focused desktop browser for Windows, built in C#/WPF using
Microsoft Edge WebView2 (the same Chromium engine that powers Edge).
Lightweight by design — it borrows Edge's already-installed Chromium
rather than bundling its own, so the app itself stays a few MB instead
of hundreds.

## Features
- Native Windows app with tabs (with close buttons), address bar,
  back/forward/reload
- **Real tracker/ad blocking**: downloads and caches the actual
  EasyList + EasyPrivacy filter lists (thousands of domains) on first
  run, refreshing weekly in the background
- **HTTPS auto-upgrade**: any `http://` link or typed address is
  rewritten to `https://` before navigating
- **Per-site shield toggle**: the 🛡 button shows how many trackers
  were blocked on the current site and lets you pause blocking for
  just that site
- **Session restore**: open tabs are saved on close and reopened
  automatically next launch
- Autofill / password autosave disabled by default
- Private search (DuckDuckGo) as the default engine for anything typed
  that isn't a URL
- Isolated user-data folder — not tied to any system Edge profile

## Requirements
- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- WebView2 Runtime (already installed on Windows 11; on Windows 10, the
  [Evergreen Bootstrapper](https://developer.microsoft.com/microsoft-edge/webview2/)
  installs it automatically if missing)
- Internet access on first run, to download the EasyList/EasyPrivacy
  filter lists (cached afterward at
  `%LOCALAPPDATA%\PrivBrowser\FilterLists\`)

## Build & run

```bash
cd privbrowser
dotnet restore
dotnet run
```

Must be built on Windows (or via a `windows-latest` GitHub Actions
runner) — WPF's XAML compiler doesn't run on Linux/Codespaces.

## Next steps
- Add a full shields panel (per-site breakdown of what was blocked)
- Add tab drag-to-reorder, bookmarks, and a history page
- Add a per-site allowlist UI instead of the current all-or-nothing
  per-site toggle
