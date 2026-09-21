# PrivBrowser

A privacy-focused desktop browser for Windows, built in C#/WPF on top of
**CefSharp** — a .NET wrapper around the real, open-source Chromium
Embedded Framework (CEF), not Microsoft's Edge-flavored WebView2.

## Features
- Native Windows app with tabs (with close buttons), address bar,
  back/forward/reload
- **Real tracker/ad blocking**: downloads and caches the actual
  EasyList + EasyPrivacy filter lists (thousands of domains) on first
  run, refreshing weekly in the background
- **HTTPS auto-upgrade**: any `http://` navigation is rewritten to
  `https://` before it happens
- **Per-site shield toggle**: the 🛡 button opens a Shields panel showing
  a per-domain breakdown of what was blocked on the current page, plus a
  checkbox to permanently allow/block this site — saved to disk, so it
  applies every time you revisit, not just for the current tab session
- **Bookmarks**: ☆ star button bookmarks/unbookmarks the current page;
  📑 opens a bookmarks list (double-click to open, or remove)
- **History**: 🕘 opens a reverse-chronological history list
  (double-click to reopen a page, or clear it entirely)
- **Session restore**: open tabs are saved on close and reopened next
  launch
- Isolated Chromium cache/profile folder, separate from any other
  browser on the machine
- Private search (DuckDuckGo) as the default engine for anything typed
  that isn't a URL
- `disable-background-networking` set on the embedded Chromium instance

## Why CefSharp instead of WebView2
WebView2 borrows whatever Chromium build ships with Edge on the user's
machine. CefSharp bundles its own Chromium binaries directly into the
app, so it isn't dependent on Edge at all — the tradeoff is a much
larger download (~400MB+), since Chromium itself now ships inside the
app instead of being borrowed from Edge.

## Requirements
- Windows 10 or 11, x64
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Internet access on first run (to fetch EasyList/EasyPrivacy; cached
  afterward under `%LOCALAPPDATA%\PrivBrowser\`)

## Build & run

```bash
cd privbrowser
dotnet restore
dotnet run
```

Must be built on Windows (or a `windows-latest` GitHub Actions runner)
— cannot be cross-compiled from Linux/Codespaces, and CefSharp requires
x64 specifically (no AnyCPU support).

## Next steps
- Add tab drag-to-reorder
- Add a bookmarks bar (not just a list) for one-click access
- Add search/filter within the history and bookmarks windows
