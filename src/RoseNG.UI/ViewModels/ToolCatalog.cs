using RoseNG.Core;
using RoseNG.Core.Discord;
using RoseNG.Core.Network;
using RoseNG.Core.Osint;

namespace RoseNG.UI.ViewModels;

public static class ToolCatalog
{
    public static IReadOnlyList<ToolDescriptor> All { get; } = new List<ToolDescriptor>
    {
        // ---- OSINT ----
        new()
        {
            Name = "WHOIS Lookup", Category = "OSINT",
            Description = "Registrar, dates, and nameservers for a domain.",
            Keywords = new[] { "domain", "registrar", "owner" },
            InputPlaceholder = "example.com",
            Execute = (input, ct) => WhoisTool.LookupAsync(input, ct)
        },
        new()
        {
            Name = "DNS Lookup", Category = "OSINT",
            Description = "Resolve A/AAAA records for a hostname.",
            Keywords = new[] { "a record", "aaaa", "resolve" },
            InputPlaceholder = "example.com",
            Execute = (input, ct) => DnsTool.ResolveAsync(input, ct)
        },
        new()
        {
            Name = "Reverse DNS", Category = "OSINT",
            Description = "Find the hostname behind an IP address.",
            Keywords = new[] { "ptr", "ip to host" },
            InputPlaceholder = "8.8.8.8",
            Execute = (input, ct) => DnsTool.ReverseLookupAsync(input, ct)
        },
        new()
        {
            Name = "HTTP Headers", Category = "OSINT",
            Description = "Inspect the response headers a site returns.",
            Keywords = new[] { "http", "headers", "server" },
            InputPlaceholder = "https://example.com",
            Execute = (input, ct) => HttpInspectTool.GetHeadersAsync(input, ct)
        },
        new()
        {
            Name = "TLS Certificate", Category = "OSINT",
            Description = "View the SSL/TLS certificate presented by a host.",
            Keywords = new[] { "ssl", "cert", "https" },
            InputPlaceholder = "example.com",
            Execute = (input, ct) => HttpInspectTool.GetTlsCertAsync(input, 443, ct)
        },
        new()
        {
            Name = "Hash Text (SHA-256)", Category = "OSINT",
            Description = "Generate a SHA-256 digest of typed text.",
            Keywords = new[] { "hash", "sha256", "checksum" },
            InputPlaceholder = "Text to hash",
            Execute = (input, _) => Task.FromResult(HashTool.HashText(input, HashAlgorithmKind.Sha256))
        },
        new()
        {
            Name = "File Metadata", Category = "OSINT",
            Description = "View filesystem metadata (and dimensions for PNG/JPEG) for a local file.",
            Keywords = new[] { "exif", "file info" },
            InputPlaceholder = "/path/to/file.png",
            Execute = (input, _) => Task.FromResult(MetadataTool.Inspect(input))
        },

        // ---- Network ----
        new()
        {
            Name = "Ping", Category = "Network",
            Description = "Send ICMP echo requests to a host.",
            Keywords = new[] { "icmp", "latency" },
            InputPlaceholder = "example.com",
            Execute = (input, ct) => NetworkTools.PingAsync(input, 4, ct)
        },
        new()
        {
            Name = "Traceroute", Category = "Network",
            Description = "Trace the route packets take to a host.",
            Keywords = new[] { "trace", "hops", "route" },
            InputPlaceholder = "example.com",
            Execute = (input, ct) => NetworkTools.TracerouteAsync(input, 30, ct)
        },
        new()
        {
            Name = "Port Scan (common ports)", Category = "Network",
            Description = "Scan common TCP ports on a host you're authorised to test.",
            Keywords = new[] { "nmap", "scan", "open ports" },
            InputPlaceholder = "192.168.1.1",
            Execute = (input, ct) => NetworkTools.ScanPortsAsync(
                input, new[] { 21, 22, 23, 25, 53, 80, 110, 143, 443, 3306, 3389, 8080 }, 500, ct)
        },
        new()
        {
            Name = "Connection Test", Category = "Network",
            Description = "Check whether a host:port is reachable.",
            Keywords = new[] { "tcp", "reachability" },
            InputPlaceholder = "example.com:443",
            Execute = (input, ct) =>
            {
                var parts = input.Split(':');
                var host = parts[0];
                var port = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : 443;
                return NetworkTools.TestConnectionAsync(host, port, ct);
            }
        },
        new()
        {
            Name = "Local Network Info", Category = "Network",
            Description = "List active local network interfaces and addresses.",
            Keywords = new[] { "interfaces", "ipconfig", "ifconfig" },
            InputPlaceholder = "(no input needed - just run)",
            Execute = (_, _) => Task.FromResult(NetworkTools.GetLocalNetworkInfo())
        },
        new()
        {
            Name = "Public IP", Category = "Network",
            Description = "Look up your current public-facing IP address.",
            Keywords = new[] { "external ip", "whatismyip" },
            InputPlaceholder = "(no input needed - just run)",
            Execute = (_, ct) => NetworkTools.GetPublicIpAsync(ct)
        },

        // ---- Discord ----
        new()
        {
            Name = "Snowflake Decoder", Category = "Discord",
            Description = "Decode a Discord ID into its creation timestamp.",
            Keywords = new[] { "id", "timestamp", "epoch" },
            InputPlaceholder = "123456789012345678",
            Execute = (input, _) => Task.FromResult(SnowflakeTool.Decode(input))
        },
        new()
        {
            Name = "Timestamp Generator", Category = "Discord",
            Description = "Build a <t:...> markdown timestamp for right now.",
            Keywords = new[] { "time", "markdown timestamp" },
            InputPlaceholder = "(no input needed - just run)",
            Execute = (_, _) => Task.FromResult(SnowflakeTool.BuildTimestamp(DateTime.Now))
        },
        new()
        {
            Name = "Webhook Test", Category = "Discord",
            Description = "Verify a webhook URL you own responds correctly.",
            Keywords = new[] { "webhook", "test" },
            InputPlaceholder = "https://discord.com/api/webhooks/...",
            Execute = (input, ct) => WebhookTool.TestAsync(input, ct)
        },
        new()
        {
            Name = "Markdown: Bold", Category = "Discord",
            Description = "Wrap text in Discord bold markdown.",
            Keywords = new[] { "format", "**" },
            InputPlaceholder = "Text to format",
            Execute = (input, _) => Task.FromResult(ToolResult.Ok(MarkdownTool.Bold(input)))
        },
    };
}
