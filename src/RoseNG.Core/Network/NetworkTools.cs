using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace RoseNG.Core.Network;

public static class NetworkTools
{
    public static async Task<ToolResult> PingAsync(string host, int count = 4, CancellationToken ct = default)
    {
        try
        {
            using var ping = new Ping();
            var sb = new StringBuilder();
            var times = new List<long>();

            for (int i = 0; i < count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var reply = await ping.SendPingAsync(host, 3000);
                if (reply.Status == IPStatus.Success)
                {
                    sb.AppendLine($"Reply from {reply.Address}: time={reply.RoundtripTime}ms TTL={reply.Options?.Ttl ?? 0}");
                    times.Add(reply.RoundtripTime);
                }
                else
                {
                    sb.AppendLine($"Request {i + 1}: {reply.Status}");
                }
            }

            if (times.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"Sent={count} Received={times.Count} Lost={count - times.Count}");
                sb.AppendLine($"Min={times.Min()}ms Max={times.Max()}ms Avg={times.Average():F1}ms");
            }

            return ToolResult.Ok(sb.ToString().TrimEnd());
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Ping failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Cross-platform traceroute implemented with incrementing TTL ICMP echoes,
    /// since the OS-native traceroute/tracert binaries differ per platform.
    /// </summary>
    public static async Task<ToolResult> TracerouteAsync(string host, int maxHops = 30, CancellationToken ct = default)
    {
        try
        {
            var target = (await Dns.GetHostAddressesAsync(host, ct)).FirstOrDefault();
            if (target is null) return ToolResult.Fail("Could not resolve host.");

            using var ping = new Ping();
            var sb = new StringBuilder();
            var buffer = Encoding.ASCII.GetBytes("rose-ng-traceroute");

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                ct.ThrowIfCancellationRequested();
                var options = new PingOptions(ttl, true);
                var reply = await ping.SendPingAsync(target, 2000, buffer, options);

                var addr = reply.Address?.ToString() ?? "*";
                sb.AppendLine($"{ttl,2}  {addr,-20}  {(reply.Status == IPStatus.TimedOut ? "timed out" : $"{reply.RoundtripTime}ms")}");

                if (reply.Status == IPStatus.Success)
                    break;
            }

            return ToolResult.Ok(sb.ToString().TrimEnd());
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Traceroute failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Port scan. Callers are responsible for only scanning hosts they are
    /// authorised to test - see README security notice.
    /// </summary>
    public static async Task<ToolResult> ScanPortsAsync(string host, IEnumerable<int> ports, int timeoutMs = 500, CancellationToken ct = default)
    {
        var open = new List<int>();
        var sb = new StringBuilder();

        foreach (var port in ports)
        {
            ct.ThrowIfCancellationRequested();
            using var client = new TcpClient();
            try
            {
                var connectTask = client.ConnectAsync(host, port, ct).AsTask();
                var completed = await Task.WhenAny(connectTask, Task.Delay(timeoutMs, ct));
                if (completed == connectTask && client.Connected)
                {
                    open.Add(port);
                    sb.AppendLine($"Port {port}: OPEN");
                }
            }
            catch
            {
                // closed/filtered - skip silently, summarised below
            }
        }

        sb.AppendLine();
        sb.AppendLine($"{open.Count} open port(s) found.");
        return ToolResult.Ok(sb.ToString().TrimEnd(), new Dictionary<string, string> { ["OpenCount"] = open.Count.ToString() });
    }

    public static ToolResult GetLocalNetworkInfo()
    {
        var sb = new StringBuilder();
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up) continue;
            sb.AppendLine($"Interface: {nic.Name} ({nic.NetworkInterfaceType})");
            foreach (var addr in nic.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6)
                    sb.AppendLine($"  {addr.Address} / {addr.IPv4Mask}");
            }
            sb.AppendLine();
        }
        return ToolResult.Ok(sb.ToString().TrimEnd());
    }

    public static async Task<ToolResult> GetPublicIpAsync(CancellationToken ct = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };
            var ip = await client.GetStringAsync("https://api.ipify.org", ct);
            return ToolResult.Ok(ip.Trim());
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Could not reach public IP service: {ex.Message}");
        }
    }

    public static async Task<ToolResult> TestConnectionAsync(string host, int port, CancellationToken ct = default)
    {
        try
        {
            using var client = new TcpClient();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await client.ConnectAsync(host, port, ct);
            sw.Stop();
            return ToolResult.Ok($"Connected to {host}:{port} in {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Connection failed: {ex.Message}");
        }
    }
}
