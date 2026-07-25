using System.Net;
using System.Net.Sockets;

namespace RoseNG.Core.Osint;

public static class DnsTool
{
    /// <summary>
    /// Resolves A/AAAA + reverse PTR using the built-in .NET resolver.
    /// This intentionally avoids a raw DNS wire-protocol implementation
    /// for V1 - it covers the 90% case (host -> IP, IP -> host) reliably
    /// on both Windows and Linux without a third dependency/language.
    /// </summary>
    public static async Task<ToolResult> ResolveAsync(string host, CancellationToken ct = default)
    {
        host = host.Trim();
        if (string.IsNullOrWhiteSpace(host))
            return ToolResult.Fail("Host is required.");

        try
        {
            var entry = await Dns.GetHostEntryAsync(host, ct);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Canonical name: {entry.HostName}");
            sb.AppendLine();

            var v4 = entry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToList();
            var v6 = entry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetworkV6).ToList();

            if (v4.Count > 0)
            {
                sb.AppendLine("A records:");
                foreach (var a in v4) sb.AppendLine($"  {a}");
            }
            if (v6.Count > 0)
            {
                sb.AppendLine("AAAA records:");
                foreach (var a in v6) sb.AppendLine($"  {a}");
            }

            return ToolResult.Ok(sb.ToString().TrimEnd());
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"DNS resolution failed: {ex.Message}");
        }
    }

    public static async Task<ToolResult> ReverseLookupAsync(string ip, CancellationToken ct = default)
    {
        if (!IPAddress.TryParse(ip.Trim(), out var address))
            return ToolResult.Fail("Enter a valid IP address for reverse lookup.");

        try
        {
            var entry = await Dns.GetHostEntryAsync(address);
            return ToolResult.Ok(entry.HostName);
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Reverse lookup failed: {ex.Message}");
        }
    }
}
