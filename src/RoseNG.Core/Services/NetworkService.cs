using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RoseNG.Core.Services
{
    public static class NetworkService
    {
        public static async Task<string> PingAsync(string host)
        {
            using var ping = new Ping();
            try
            {
                var reply = await ping.SendPingAsync(host, 3000);
                return $"Status: {reply.Status}\nRTT: {reply.RoundtripTime} ms\nTTL: {reply.Options?.Ttl}";
            }
            catch (Exception ex)
            {
                return $"Ping failed: {ex.Message}";
            }
        }

        public static async Task<string> TracerouteAsync(string host)
        {
            var sb = new StringBuilder();
            using var ping = new Ping();
            const int maxHops = 30;

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                var options = new PingOptions(ttl, true);
                var buffer = new byte[32];
                try
                {
                    var reply = await ping.SendPingAsync(host, 2000, buffer, options);
                    sb.AppendLine($"{ttl,2}  {reply.Address}  {reply.RoundtripTime} ms  [{reply.Status}]");
                    if (reply.Status == IPStatus.Success) break;
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"{ttl,2}  error: {ex.Message}");
                    break;
                }
            }
            return sb.ToString();
        }

        public static async Task<string> PortScanAsync(string host, int startPort, int endPort)
        {
            var sb = new StringBuilder();
            var openPorts = new List<int>();

            var tasks = new List<Task>();
            for (int port = startPort; port <= endPort; port++)
            {
                int p = port;
                tasks.Add(Task.Run(async () =>
                {
                    using var client = new TcpClient();
                    try
                    {
                        var connectTask = client.ConnectAsync(host, p);
                        if (await Task.WhenAny(connectTask, Task.Delay(500)) == connectTask && client.Connected)
                        {
                            lock (openPorts) openPorts.Add(p);
                        }
                    }
                    catch { /* closed/filtered */ }
                }));
            }
            await Task.WhenAll(tasks);
            openPorts.Sort();

            sb.AppendLine($"Scanned {host} ports {startPort}-{endPort}");
            sb.AppendLine(openPorts.Count == 0 ? "No open ports found." : "Open ports:");
            foreach (var p in openPorts)
                sb.AppendLine($"  {p}/tcp open");

            return sb.ToString();
        }

        // Active ARP sweep: ping every host on the local /24 to populate the OS's ARP
        // cache, then read that cache back via the platform-native tool (there's no
        // cross-platform managed API for the ARP table, so this shells out).
        public static async Task<string> ArpSweepAsync(bool activeSweep = true)
        {
            var sb = new StringBuilder();

            if (activeSweep)
            {
                var subnet = GetLocalSubnetPrefix();
                if (subnet != null)
                {
                    sb.AppendLine($"Pinging {subnet}.0/24 to populate the ARP cache...");
                    var pingTasks = new List<Task>();
                    for (int host = 1; host <= 254; host++)
                    {
                        var target = $"{subnet}.{host}";
                        pingTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                using var ping = new Ping();
                                await ping.SendPingAsync(target, 300);
                            }
                            catch { /* unreachable/filtered hosts are expected */ }
                        }));
                    }
                    await Task.WhenAll(pingTasks);
                    sb.AppendLine("Sweep complete.");
                }
                else
                {
                    sb.AppendLine("Could not determine local IPv4 subnet; skipping active sweep, reading cache only.");
                }
                sb.AppendLine();
            }

            sb.AppendLine("ARP cache:");
            sb.AppendLine(await ReadOsArpCacheAsync());
            return sb.ToString();
        }

        private static string? GetLocalSubnetPrefix()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                foreach (var addr in nic.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                    var b = addr.Address.GetAddressBytes();
                    if (b[0] == 127) continue;
                    return $"{b[0]}.{b[1]}.{b[2]}";
                }
            }
            return null;
        }

        private static async Task<string> ReadOsArpCacheAsync()
        {
            string fileName;
            string arguments;

            if (OperatingSystem.IsWindows())
            {
                fileName = "arp";
                arguments = "-a";
            }
            else if (OperatingSystem.IsMacOS())
            {
                fileName = "arp";
                arguments = "-a";
            }
            else if (OperatingSystem.IsLinux())
            {
                fileName = "ip";
                arguments = "neigh";
            }
            else
            {
                return "Unsupported platform for ARP cache reading.";
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var proc = Process.Start(psi);
                if (proc == null) return $"Failed to start '{fileName} {arguments}'.";

                string output = await proc.StandardOutput.ReadToEndAsync();
                string error = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();

                if (string.IsNullOrWhiteSpace(output))
                    return string.IsNullOrWhiteSpace(error) ? "(empty ARP cache)" : $"'{fileName} {arguments}' failed: {error.Trim()}";

                return output.Trim();
            }
            catch (Exception ex)
            {
                return $"Failed to read ARP cache: {ex.Message}\n" +
                       $"('{fileName} {arguments}' must be available on PATH.)";
            }
        }

        public static string MacVendorLookup(string mac)
        {
            var cleaned = mac.Replace(":", "").Replace("-", "").ToUpperInvariant();
            if (cleaned.Length < 6) return "Invalid MAC address";

            var oui = cleaned.Substring(0, 6);
            var vendor = OuiDatabase.Lookup(oui);

            if (vendor != null)
                return $"OUI: {oui}\nVendor: {vendor}";

            return $"OUI: {oui}\nVendor: unknown (not in the bundled {OuiDatabase.EntryCount}-entry OUI subset)\n" +
                   $"For full IEEE coverage, download oui.csv from https://standards-oui.ieee.org/oui/oui.csv " +
                   $"and place it at:\n{OuiDatabase.ExtraDatabasePath}";
        }
        public static async Task SendWakeOnLanAsync(string macAddress)
        {
            var mac = macAddress.Replace(":", "").Replace("-", "");
            var macBytes = Convert.FromHexString(mac);

            var packet = new byte[6 + 16 * macBytes.Length];
            for (int i = 0; i < 6; i++) packet[i] = 0xFF;
            for (int i = 0; i < 16; i++)
                Array.Copy(macBytes, 0, packet, 6 + i * macBytes.Length, macBytes.Length);

            using var client = new UdpClient();
            client.EnableBroadcast = true;
            await client.SendAsync(packet, packet.Length, new IPEndPoint(IPAddress.Broadcast, 9));
        }

        public static string LocalInterfaceInfo()
        {
            var sb = new StringBuilder();
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                sb.AppendLine($"{nic.Name} ({nic.NetworkInterfaceType})");
                sb.AppendLine($"  MAC: {nic.GetPhysicalAddress()}");
                foreach (var addr in nic.GetIPProperties().UnicastAddresses)
                    sb.AppendLine($"  IP:  {addr.Address}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static async Task<string> HttpHeaderGrabAsync(string url)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(8);
            var sb = new StringBuilder();
            try
            {
                var resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                sb.AppendLine($"{(int)resp.StatusCode} {resp.StatusCode}");
                foreach (var h in resp.Headers)
                    sb.AppendLine($"{h.Key}: {string.Join(", ", h.Value)}");
                foreach (var h in resp.Content.Headers)
                    sb.AppendLine($"{h.Key}: {string.Join(", ", h.Value)}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Request failed: {ex.Message}");
            }
            return sb.ToString();
        }
    }
}
