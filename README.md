# rose-ng

A cross-platform (Windows + Linux) desktop multi-tool for OSINT, Discord, and network
utilities. Built as a focused V1.0 MVP: a small set of tools done properly, on a
foundation designed to grow, rather than a sprawling toolbox done poorly.

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
reliable tools grouped into three categories for V1.0: **OSINT**, **Discord**,
and **Network**. Every tool follows the same shape - pick it from the search
bar, give it one input, hit Run, read the output - so the app stays simple to
use even as more tools are added later.

**Design goals for V1.0:**

- A small, *working* set of tools rather than a large set of half-finished ones
- A clean separation between UI (`RoseNG.UI`) and logic (`RoseNG.Core`), so a
  new tool is normally just one new file + one catalog entry - no UI rewrite
- Native look and feel on both Windows and Linux from one codebase
- A dark theme that doesn't feel like a rebranded default template
- No telemetry, no accounts, no cloud dependency - it's a local utility

## Features

### OSINT
- **WHOIS lookup** - registrar, dates, and referral-chain resolution (via raw RFC 3912 socket, no third-party API)
- **DNS lookup** - A/AAAA record resolution
- **Reverse DNS** - IP → hostname
- **HTTP header inspection** - see exactly what headers a server returns
- **TLS certificate viewer** - subject, issuer, validity window, thumbprint
- **Hash generation** - MD5 / SHA-1 / SHA-256 / SHA-512 for text or files
- **Hash verification** - compare a computed hash against an expected one
- **File metadata viewer** - filesystem metadata, plus PNG/JPEG dimensions

### Discord *(all features comply with Discord's Terms of Service)*
- **Webhook testing** - validate a webhook URL **you own** before using it
- **Webhook message sending** - post content/embeds to your own webhook
- **Snowflake decoder** - turn any Discord ID into its creation timestamp
- **Timestamp generator** - build `<t:unix:format>` markdown timestamps
- **Markdown formatting helpers** - bold/italic/underline/strikethrough/spoiler/code block/quote
- **Embed builder** *(backend model included; full visual builder UI ships in v1.1 - see [Roadmap](#roadmap))*

> rose-ng never scrapes, brute-forces, or discovers webhooks/tokens that
> don't belong to the user. Every Discord feature operates on data you
> explicitly provide.

### Network
- **Ping** - ICMP echo with latency/loss summary
- **Traceroute** - TTL-incrementing route trace (implemented in-app so it behaves identically on Windows and Linux, rather than shelling out to `tracert`/`traceroute`)
- **Port scan** - common-port TCP scan, **for hosts you're authorised to test**
- **Connection test** - quick TCP reachability + latency check
- **Local network info** - active interfaces and assigned addresses
- **Public IP lookup** - your current externally-visible IP

### Explicitly out of scope for V1.0
Plugin marketplaces, cloud sync, AI integrations, scripting/automation
pipelines, and long tails of niche tools are intentionally deferred - see
[Roadmap](#roadmap). V1.0 is meant to feel *complete* at a small scope, not
*incomplete* at a large one.

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

**Languages used:** C# for both UI and backend logic, and Python for the
tiny standalone `tools/regen-invite.py` maintenance script (not part of the
shipped app). That's 2 of the 3 languages budgeted for this project, leaving
room for a performance-critical component in a systems language later
(e.g. Rust/Go) if a future tool genuinely needs it.

## The support-link mechanism

The app links to a Discord support/community server. Per your request, that
invite URL is not stored as plain, greppable text in the repository:

- The real URL lives only as an XOR-obfuscated byte array in
  `RoseNG.Core/Security/Links.cs`.
- `RoseNG.Core/Security/ObfuscatedString.cs` reverses the obfuscation **at
  runtime only**, right before the link is opened.
- To change the invite, run `python3 tools/regen-invite.py "<new-url>"` and
  paste the printed byte array over the existing one in `Links.cs`. That's
  the entire rotation process.

**Please read this honestly:** this is *obfuscation*, not encryption. Any
string shipped inside a public, unauthenticated binary can eventually be
recovered by someone willing to attach a debugger or disassembler - there's
no cryptographic scheme that changes that for a plain support-server invite,
since the app itself must be able to decode it with no secret key of its
own. What this approach *does* achieve, and the reason it's worth doing:
- the literal invite text won't appear in `git log`, `grep -r`, or a casual
  `strings RoseNG.exe`
- automated scrapers crawling public GitHub repos for `discord.gg/...`
  patterns won't pick it up
- rotating it is a one-line change, not a re-plumbing of the codebase

If you need real protection against invite scraping/raiding, pair this with
Discord's own server-side controls (member screening, invite expiry, verification levels) - those provide actual security; this mechanism only provides discretion.

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
