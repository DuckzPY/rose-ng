# rose-ng

A dark-themed, offline-first hacking/OSINT toolbox desktop app, built with Avalonia (.NET).
Everything is local — no telemetry, no accounts, rolling release.

## Running it

Requires the .NET SDK (targets `net10.0` — adjust `TargetFramework` in `RoseNg.csproj`
if you're on a different SDK).

```bash
dotnet run
```

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

## Project layout

```
RoseNg.csproj        project file (net10.0, Avalonia)
Program.cs           entry point
App.axaml(.cs)        app-wide theme/styles
MainWindow.axaml(.cs) home screen, tile grid, navigation
Services/             all tool logic (no UI dependencies)
Views/                one UserControl + tab set per category
```

## Known stubs/fixes

- **Breach check** (OSINT) — implemented against the HaveIBeenPwned v3 API.
  Requires your own paid API key (https://haveibeenpwned.com/API/Key), entered
  once in the tool and persisted locally in `settings.json`.
- **MAC vendor lookup** (Network) — implemented against a bundled curated
  subset of common IEEE OUI vendor prefixes. Not exhaustive; for full
  registry coverage, drop the official `oui.csv` (from
  https://standards-oui.ieee.org/oui/oui.csv) next to `settings.json` in the
  app's data folder as `oui-extra.csv`.
- **ARP sweep** (Network) — implemented as a ping sweep of the local /24
  followed by a read of the OS ARP cache (`arp -a` on Windows/macOS,
  `ip neigh` on Linux).

## License / disclaimer

For your own systems, bots, and servers, or with explicit authorization. Not
a substitute for actual authorization to test something you don't own.
