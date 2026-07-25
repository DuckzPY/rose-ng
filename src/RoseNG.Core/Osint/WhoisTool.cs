using System.Net.Sockets;
using System.Text;

namespace RoseNG.Core.Osint;

/// <summary>
/// Minimal RFC 3912 WHOIS client. Talks directly to port 43 so it has
/// zero external dependencies and works identically on Windows/Linux.
/// </summary>
public static class WhoisTool
{
    private const int WhoisPort = 43;
    private const string DefaultServer = "whois.iana.org";

    public static async Task<ToolResult> LookupAsync(string target, CancellationToken ct = default)
    {
        target = target.Trim();
        if (string.IsNullOrWhiteSpace(target))
            return ToolResult.Fail("Target domain/IP is required.");

        try
        {
            // IANA acts as a root referral service; if the response contains
            // a "refer:" line we follow it once to get the authoritative registrar data.
            var first = await QueryAsync(DefaultServer, target, ct);
            var referServer = ExtractReferral(first);

            if (!string.IsNullOrEmpty(referServer) && referServer != DefaultServer)
            {
                var second = await QueryAsync(referServer, target, ct);
                return ToolResult.Ok(second, new Dictionary<string, string>
                {
                    ["Server"] = referServer
                });
            }

            return ToolResult.Ok(first, new Dictionary<string, string> { ["Server"] = DefaultServer });
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"WHOIS lookup failed: {ex.Message}");
        }
    }

    private static async Task<string> QueryAsync(string server, string target, CancellationToken ct)
    {
        using var client = new TcpClient();
        client.ReceiveTimeout = 8000;
        await client.ConnectAsync(server, WhoisPort, ct);

        await using var stream = client.GetStream();
        var request = Encoding.ASCII.GetBytes(target + "\r\n");
        await stream.WriteAsync(request, ct);

        using var reader = new StreamReader(stream, Encoding.ASCII);
        return await reader.ReadToEndAsync(ct);
    }

    private static string? ExtractReferral(string whoisResponse)
    {
        foreach (var line in whoisResponse.Split('\n'))
        {
            if (line.StartsWith("refer:", StringComparison.OrdinalIgnoreCase))
                return line.Split(':', 2)[1].Trim();
        }
        return null;
    }
}
