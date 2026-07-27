# rose-ng

A offline-first hacking/OSINT toolbox desktop app, built with Avalonia (.NET).
Everything is local || no telemetry, no accounts, rolling release.

rose-ng is the C#/.NET rewrite of [rose-fg](https://github.com/DuckzPY/rose-fg)
(the original CustomTkinter/Python toolkit), aimed at a native, single-binary
desktop experience on both major desktop OSes.

## Table of contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Building from source](#building-from-source)
- [Usage](#usage)
- [Project structure](#project-structure)
- [Dependencies](#dependencies)
- [The support-link mechanism](#the-support-link-mechanism)
- [Troubleshooting](#troubleshooting)
- [Roadmap](#roadmap)
- [Security & responsible use](#security--responsible-use)
- [License](#license)
- [Credits](#credits)

## Overview

rose-ng is one desktop app, one search bar, and a growing catalog of small,
reliable tools grouped into three categories for V2.0: **OSINT**, **Discord**,
and **Network**. Every tool follows the same shape - pick it from the search
bar, give it one input, hit Run, read the output - so the app stays simple to
use even as more tools are added later.

## Features

### OSINT
- WHOIS lookup (raw TCP query, port 43)
- DNS lookup (A / AAAA / MX / TXT / NS / CNAME)
- SSL/TLS certificate inspector
- Subnet / CIDR calculator
- Reverse IP (PTR) lookup
- IP geolocation
- ASN lookup
- robots.txt fetcher
- Wayback Machine snapshot check
- Username search across platforms (GitHub, X, Instagram, Reddit, TikTok)

### Network
- Ping
- Traceroute
- Concurrent async port scanner
- HTTP header / banner grabber
- Wake-on-LAN packet sender
- Local network interface info (MAC/IP per adapter)
- ARP cache reader

### Discord
- Webhook sender (plain messages)
- Embed builder (title/description/color via webhook)
- Snowflake (ID) → timestamp decoder
- Bot token format validator
- Own-bot-token API check (`/users/@me`)
- Invite resolver
- Webhook info fetch
- Own-guild emoji list
- Own-guild role list
- Own-guild audit log fetch

> Anything Discord-related here is scoped to your own bot/server/webhook. Tools
> that could be used to spam, nuke, or abuse servers you don't control aren't
> included on purpose.

### Security
- Cryptographically-secure password generator (configurable length/symbols/digits/case)
- Password strength checker
- Hash generator (MD5 / SHA1 / SHA256 / SHA512)
- Hash identifier (by length)
- Wordlist-based hash cracker
- File integrity checker (checksum compare)

### Encoding
- Base64 encode/decode
- Hex ↔ ASCII/binary
- JWT decoder (header + payload, no signature verification)
- URL encode/decode
- ROT13
- XOR cipher (encrypt/decrypt with a key)

## Installation

### Windows
1. Download the latest `rose-ng-win-x64.zip` from the [Releases](../../releases) page.
2. Extract it anywhere.
3. Run `RoseNG.exe`. No installer, no admin rights required.

### Linux
1. Download the latest `rose-ng-linux-x64.tar.gz` from the [Releases](../../releases) page.
2. Extract it: `tar -xzf rose-ng-linux-x64.tar.gz`
3. Make it executable and run it:
   ```bash
   chmod +x RoseNG
   ./RoseNG
   ```

Both packages are self-contained, single-directory .NET publishes - no separate
.NET runtime install is required for the release builds.

## Building from source

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download) (Windows or Linux)
- Git

### Clone and restore
```bash
git clone https://github.com/DuckzPY/rose-ng.git
cd rose-ng
dotnet restore
```

### Run in development
```bash
dotnet run --project src/RoseNG.UI
```

### Publish a release build

Windows (from Windows or cross-compiled from Linux):
```bash
dotnet publish src/RoseNG.UI -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64
```

Linux:
```bash
dotnet publish src/RoseNG.UI -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/linux-x64
```

> **Note on this repository as delivered:** the solution has been authored
> and reviewed but not compiled in the sandbox that generated it (no NuGet
> access there). Run `dotnet restore` locally before your first build - if
> anything doesn't compile cleanly, please open an issue with the error, as
> that's a bug in this scaffold, not something you did wrong.

## Usage

1. Launch the app. The search bar at the top filters tools by name, category,
   description, or keyword as you type (e.g. type "whois", "ping", or "hash").
2. Select a tool from the left-hand list.
3. Enter the required input (a domain, IP, file path, etc. - each tool shows
   a placeholder describing what it expects) and click **Run**.
4. Read the result in the output panel. Errors are shown in place rather than
   as a separate dialog, so you can adjust input and re-run quickly.

Tools that don't need an input (like **Local Network Info** or **Public IP**)
can be run directly - just select and click Run.

## Project structure

```
rose-ng/
├── RoseNG.sln
├── src/
│   ├── RoseNG.Core/              # Backend logic - no UI references, fully testable
│   │   ├── Osint/                # WHOIS, DNS, HTTP/TLS inspection, hashing, metadata
│   │   ├── Network/               # Ping, traceroute, port scan, connection test, etc.
│   │   ├── Discord/               # Snowflake decode, webhooks, markdown helpers
│   │   ├── Security/               # Obfuscated-link helper (see below)
│   │   └── IToolResult.cs          # Shared ToolResult envelope every tool returns
│   └── RoseNG.UI/                 # Avalonia UI - the only project referencing a UI framework
│       ├── Views/                  # XAML windows/pages
│       ├── ViewModels/             # MVVM view models + the tool catalog
│       └── Styles/                 # Dark theme resource dictionary
├── tools/
│   └── regen-invite.py             # Rotates the obfuscated Discord support link
├── LICENSE
└── README.md
```

**Adding a new tool in a future version** means: write the logic as a static
method in `RoseNG.Core` returning `ToolResult`, then add one `ToolDescriptor`
entry to `ToolCatalog.All` in the UI project. The search bar, list, and run
panel all pick it up automatically - no other UI changes needed.

## Dependencies

**RoseNG.Core** (backend) - .NET 8 base class library only. No third-party
packages, by design, so the core logic stays lightweight, auditable, and
trivially portable.

**RoseNG.UI** (frontend):
| Package | Purpose |
|---|---|
| Avalonia | Cross-platform XAML UI framework (the reason Windows *and* Linux work from one codebase) |
| Avalonia.Desktop | Desktop platform backend for Avalonia |
| Avalonia.Themes.Fluent | Base theme, customised further in `Styles/Theme.axaml` |
| Avalonia.Fonts.Inter | Bundled UI font so text renders consistently across OSes |
| CommunityToolkit.Mvvm | Source-generated `ObservableObject`/`RelayCommand` for clean MVVM without boilerplate |

## Troubleshooting

**"dotnet: command not found"** - Install the .NET 8 SDK from
[dotnet.microsoft.com](https://dotnet.microsoft.com/download) and ensure it's
on your `PATH`.

**App won't launch on Linux ("permission denied")** - Run `chmod +x RoseNG`
on the extracted binary.

**Ping/Traceroute return nothing on Linux** - Some Linux distributions
restrict unprivileged raw ICMP sockets. Either run the app once with
`sudo`, or grant the capability directly so you don't need to run the whole
app as root:
```bash
sudo setcap cap_net_raw+ep ./RoseNG
```

**Port scan reports everything closed** - Check that you're targeting a host
you actually have permission to scan, and that no local firewall is blocking
outbound connections on the container/VM you're running from.

**WHOIS returns "connection refused"** - Some registries rate-limit or block
WHOIS (port 43) from certain networks/VPNs. Try again from a different
network, or use a browser-based WHOIS as a fallback.

**Webhook test/send fails with 401/404** - Double check the full webhook URL
was copied correctly from Discord's channel settings, and that the webhook
hasn't been deleted.

## Roadmap

**v1.x (near-term, expanding within existing categories)**
- Visual Discord embed builder with live preview
- Full webhook message composer (multiple embeds, file attachments)
- Configurable port-scan ranges and custom port lists
- EXIF/GPS metadata extraction for JPEG (image library integration)
- Export tool output to file (`.txt`/`.json`)
- Update checker (opt-in, checks GitHub releases only - no telemetry)

**v2.0 (bigger swings, deliberately deferred out of V1.0)**
- Settings panel: theme variants, default timeout tuning, keyboard shortcuts
- Plugin system for community-contributed tools
- Additional categories (Security, Encoding, Dev tools, Notes) carried over
  from rose-fg's scope, rebuilt natively rather than ported wholesale
- Session history / saved results
- Optional cloud sync (strictly opt-in if it ever ships)

Anything not listed above (AI features, always-on automation, marketplaces)
is intentionally out of scope indefinitely, not just for V1.0.

## Security & responsible use

- **Port scanning and connection testing** are provided for testing systems
  and networks you own or are explicitly authorised to assess. Scanning
  third-party infrastructure without authorisation may violate local law and
  the target's terms of service - that's on the user, not the tool.
- **Discord features** only act on webhooks/data the user supplies directly
  and are designed to stay within Discord's Terms of Service (no scraping,
  no token discovery, no automation of user accounts).
- **OSINT tools** here query public WHOIS/DNS/HTTP infrastructure the way
  any browser or command-line client would - no private databases, no paid
  data broker integrations.

## License

Released under the [MIT License](LICENSE) - free to use, modify, and
redistribute, including commercially, with attribution.

## Credits

- Built by [DuckzPY](https://github.com/DuckzPY) and [1stla](https://github.com/1stlla) 
- UI powered by [Avalonia](https://avaloniaui.net/)
- MVVM plumbing via [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- Predecessor: [rose-fg](https://github.com/DuckzPY/rose-fg)

## Disclaimer (Not that this will stop you but I have to include it)
For your own systems, bots, and servers, or with explicit authorisation. Not
a substitute for actual authorisation to test something you don't own.
