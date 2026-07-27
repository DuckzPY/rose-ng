using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RoseNG.Core
{
    public record SearchItem(string Category, string Tool, string[] Keywords);

    public static class ToolIndex
    {
        public static readonly List<SearchItem> Items = new()
        {
            new("OSINT", "WHOIS lookup", new[] { "whois", "domain", "registrar" }),
            new("OSINT", "DNS lookup", new[] { "dns", "a record", "mx", "txt", "ns", "cname" }),
            new("OSINT", "SSL inspector", new[] { "ssl", "tls", "certificate", "cert" }),
            new("OSINT", "Subnet calculator", new[] { "subnet", "cidr", "netmask" }),
            new("OSINT", "Reverse IP", new[] { "reverse ip", "ptr" }),
            new("OSINT", "Geolocation", new[] { "geolocation", "geoip", "ip location" }),
            new("OSINT", "ASN lookup", new[] { "asn", "as number", "isp" }),
            new("OSINT", "robots.txt", new[] { "robots", "robots.txt" }),
            new("OSINT", "Wayback check", new[] { "wayback", "archive", "snapshot" }),
            new("OSINT", "Username search", new[] { "username", "handle", "sherlock" }),

            new("Network", "Ping", new[] { "ping" }),
            new("Network", "Traceroute", new[] { "traceroute", "trace" }),
            new("Network", "Port scan", new[] { "port scan", "nmap", "ports" }),
            new("Network", "HTTP headers", new[] { "headers", "banner grab", "http" }),
            new("Network", "Wake-on-LAN", new[] { "wol", "wake on lan", "wake-on-lan" }),
            new("Network", "Interfaces", new[] { "interfaces", "nic", "local ip" }),
            new("Network", "ARP sweep", new[] { "arp", "arp sweep" }),

            new("Discord", "Webhook sender", new[] { "webhook", "send" }),
            new("Discord", "Embed builder", new[] { "embed" }),
            new("Discord", "Snowflake decoder", new[] { "snowflake", "discord id" }),
            new("Discord", "Token check", new[] { "token", "bot token" }),
            new("Discord", "Invite resolver", new[] { "invite", "discord.gg" }),
            new("Discord", "Webhook info", new[] { "webhook info" }),
            new("Discord", "Emoji list", new[] { "emoji", "emojis" }),
            new("Discord", "Role list", new[] { "role", "roles" }),
            new("Discord", "Audit log", new[] { "audit log", "audit" }),

            new("Security", "Password generator", new[] { "password", "generate password" }),
            new("Security", "Strength check", new[] { "strength", "password strength" }),
            new("Security", "Hash generator", new[] { "hash", "md5", "sha1", "sha256", "sha512" }),
            new("Security", "Hash identifier", new[] { "identify hash", "hash type" }),
            new("Security", "Hash cracker", new[] { "crack", "wordlist" }),
            new("Security", "File integrity", new[] { "checksum", "file integrity" }),

            new("Encoding", "Base64", new[] { "base64", "b64" }),
            new("Encoding", "Hex", new[] { "hex" }),
            new("Encoding", "JWT decoder", new[] { "jwt", "json web token" }),
            new("Encoding", "URL", new[] { "url encode", "url decode", "percent encoding" }),
            new("Encoding", "ROT13", new[] { "rot13", "caesar" }),
            new("Encoding", "XOR cipher", new[] { "xor" }),
        };

        public static List<SearchItem> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<SearchItem>();
            var q = query.Trim().ToLowerInvariant();

            return Items
                .Where(i => i.Category.ToLowerInvariant().Contains(q)
                         || i.Tool.ToLowerInvariant().Contains(q)
                         || i.Keywords.Any(k => k.Contains(q) || q.Contains(k)))
                .OrderBy(i => i.Tool.ToLowerInvariant().StartsWith(q) ? 0 : 1)
                .Take(8)
                .ToList();
        }

        /// <summary>
        /// Sniffs a pasted value and returns the best-matching category + tool,
        /// so pasting an IP, hash, JWT, etc. jumps straight to the right tool.
        /// </summary>
        public static SearchItem? DetectTarget(string input)
        {
            var value = input.Trim();
            if (value.Length == 0) return null;

            if (Regex.IsMatch(value, @"^ey[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]*$"))
                return new("Encoding", "JWT decoder", Array.Empty<string>());

            if (Regex.IsMatch(value, @"^[0-9a-fA-F]{32}$"))
                return new("Security", "Hash identifier", Array.Empty<string>()); // MD5-length

            if (Regex.IsMatch(value, @"^[0-9a-fA-F]{40}$"))
                return new("Security", "Hash identifier", Array.Empty<string>()); // SHA1-length

            if (Regex.IsMatch(value, @"^[0-9a-fA-F]{64}$"))
                return new("Security", "Hash identifier", Array.Empty<string>()); // SHA256-length

            if (Regex.IsMatch(value, @"^\d{15,20}$"))
                return new("Discord", "Snowflake decoder", Array.Empty<string>());

            if (Regex.IsMatch(value, @"^[0-9A-Fa-f]{2}([:-][0-9A-Fa-f]{2}){5}$"))
                return new("Network", "Interfaces", Array.Empty<string>()); // MAC-shaped

            if (Regex.IsMatch(value, @"^\d{1,3}(\.\d{1,3}){3}(/\d{1,2})?$"))
                return value.Contains('/')
                    ? new("OSINT", "Subnet calculator", Array.Empty<string>())
                    : new("OSINT", "Geolocation", Array.Empty<string>());

            if (value.StartsWith("https://discord.com/api/webhooks/") || value.Contains("discord.com/api/webhooks"))
                return new("Discord", "Webhook sender", Array.Empty<string>());

            if (Regex.IsMatch(value, @"^https?://", RegexOptions.IgnoreCase))
                return new("Network", "HTTP headers", Array.Empty<string>());

            if (Regex.IsMatch(value, @"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                return new("OSINT", "WHOIS lookup", Array.Empty<string>());

            return null;
        }
    }
}
