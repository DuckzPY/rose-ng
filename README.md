# rose-ng &nbsp;·&nbsp; v1.0
> A private, offline-first toolkit for network intelligence, OSINT, Discord automation, and developer utilities — built with C# and WPF.

---

## What is rose-ng?

rose-ng is a personal desktop application that consolidates a wide range of security, networking, encoding, and developer tools into a single dark-themed GUI. It is designed to be run locally with no telemetry, no accounts, and no cloud dependency. Everything runs on your machine.

It is not intended for distribution. This is a private tool.

---

## Setup & Installation

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Windows 10/11 (the app targets `net10.0-windows` and uses WPF, so it will not run on macOS or Linux without porting the UI layer)

### 1. Clone or download the project

```
git clone https://github.com/DuckzPY/rose-ng/
cd rose-ng
```

### 2. Restore dependencies

```
dotnet restore
```

rose-ng has no external NuGet dependencies beyond the .NET/WPF base — everything used (`System.Windows`, `System.Text.RegularExpressions`, etc.) ships with the SDK.

### 3. Run

```
dotnet run
```

Or launch via `launch.bat` (Windows) / `launch.sh` (Linux/macOS, if you've ported the UI) in the project root.

> **Important:** Build in Release mode (`dotnet build -c Release`) before distributing an executable — Debug builds are noticeably slower to start.

### Font Note

rose-ng uses **Segoe UI** as its base font, matching the native Windows look. No extra font installation is required.

---

## Features

Tools are organized into categories, each searchable from the home screen's search bar:

| Category | Tools |
|---|---|
| **OSINT** | WHOIS Lookup, DNS Lookup, SSL Certificate Check, Subnet Calculator |
| **Network** | Port Scan, Ping, Traceroute |
| **Discord** | Webhook Sender, Bot Token Generator |
| **Security** | Hash Generator, Password Generator, JWT Decoder |
| **Encoding** | Base64 Encode/Decode, URL Encode/Decode, Hex Converter |
| **Dev tools** | JSON Formatter, Regex Tester, Diff Checker |
| **Notes** | New Note, All Notes |

### Search
Type in the search bar to jump straight to any tool, or paste an IP/domain to get a shortlist of relevant target-aware tools (e.g. pasting an IP surfaces Port Scan, Ping, and WHOIS with that IP pre-filled).

### Notes
Persistent scratch pad for quick notes alongside your other tools.

---

## Usage Notes

- **Searchbar navigation** — tools and categories are both searchable; pasting a raw IP or domain automatically suggests relevant tools with the target pre-filled.
- **Dark theme** — the UI uses a fixed dark palette (no light/system theme toggle currently).
- **Single-window app** — categories and tools are navigated within one `MainWindow`; there are no separate pop-out windows.

---

## File Structure

```
rose-ng/
├── Program.cs         # Entry point + MainWindow (UI, layout, search, tool categories)
├── Tools.cs            # Tool implementations (network, OSINT, encoding, dev utilities, etc.)
├── rose-ng.csproj      # Project file (net10.0-windows, WPF)
├── launch.bat          # Windows launch script
├── launch.sh           # Linux/macOS launch script (requires a ported, non-WPF UI to actually run)
└── README.md           # This file
```

---

## Dependencies

rose-ng is built entirely on the .NET base class library and WPF — there are no third-party NuGet packages to install. Everything (`System.Net`, `System.Text.RegularExpressions`, `System.Security.Cryptography`, `System.Windows.*`, etc.) ships with the .NET SDK.

---

## Known Limitations

- WPF ties the UI to Windows — running on Linux/macOS requires either .NET's experimental cross-platform WPF support (not production-ready) or porting the UI to a cross-platform toolkit (Avalonia, MAUI, etc.).
- Username Checker–style results (if added) would be based on HTTP response codes only — some platforms return 200 for non-existent accounts or block bot user agents, so results may not be perfectly accurate.
- Crypto prices and exchange rates (if added) depend on free public APIs which have rate limits.
- Port Scanner is single-threaded per port — large ranges (e.g. 1–65535) will take a long time even at max speed.

---

## Privacy

- No data is sent anywhere except by the tools that explicitly make network requests (lookups, Discord API calls, etc.).
- No analytics, no logging, no crash reporting.
- Discord bot tokens are entered per-session and never written to disk.

---

*Private project — not for redistribution.*

*Join the discord here:*
```
https://discord.gg/9g3VtekQ5y
```
*Any questions or concerns dm:*
```
fzb3 on discord
```
or
```
du.ckz on discord
```
