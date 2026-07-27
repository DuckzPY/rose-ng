using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RoseNG.Core.Services
{
    public static class OsintService
    {
        private static readonly HttpClient Http = new HttpClient();

        // Raw WHOIS via TCP to the relevant registry's whois server (port 43)
        public static async Task<string> WhoisAsync(string domain, string server = "whois.iana.org")
        {
            using var client = new TcpClient();
            await client.ConnectAsync(server, 43);
            using var stream = client.GetStream();
            var query = Encoding.ASCII.GetBytes(domain + "\r\n");
            await stream.WriteAsync(query, 0, query.Length);

            using var reader = new System.IO.StreamReader(stream, Encoding.ASCII);
            return await reader.ReadToEndAsync();
        }

        public static async Task<string> DnsLookupAsync(string host)
        {
            var sb = new StringBuilder();
            try
            {
                var entry = await Dns.GetHostEntryAsync(host);
                sb.AppendLine($"Host: {entry.HostName}");
                foreach (var addr in entry.AddressList)
                    sb.AppendLine($"  {(addr.AddressFamily == AddressFamily.InterNetworkV6 ? "AAAA" : "A")}  {addr}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Lookup failed: {ex.Message}");
            }
            return sb.ToString();
        }

        public static string SubnetCalc(string cidr)
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var ip) || !int.TryParse(parts[1], out var prefix))
                return "Invalid CIDR, expected format 192.168.1.0/24";

            uint ipNum = BitConverter.ToUInt32(ip.GetAddressBytes().Reverse().ToArray(), 0);
            uint mask = prefix == 0 ? 0 : 0xFFFFFFFF << (32 - prefix);
            uint network = ipNum & mask;
            uint broadcast = network | ~mask;
            long hostCount = (long)(broadcast - network - 1);
            if (hostCount < 0) hostCount = 0;

            static string ToIp(uint n) => new IPAddress(BitConverter.GetBytes(n).Reverse().ToArray()).ToString();

            return $"Network:    {ToIp(network)}\n" +
                   $"Broadcast:  {ToIp(broadcast)}\n" +
                   $"Netmask:    {ToIp(mask)}\n" +
                   $"Usable range: {ToIp(network + 1)} - {ToIp(broadcast - 1)}\n" +
                   $"Usable hosts: {hostCount}";
        }

        public static async Task<string> SslInspectAsync(string host, int port = 443)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port);
            using var ssl = new SslStream(client.GetStream(), false, (s, cert, chain, errors) => true);
            await ssl.AuthenticateAsClientAsync(host);

            var cert2 = new X509Certificate2(ssl.RemoteCertificate!);
            return $"Subject:      {cert2.Subject}\n" +
                   $"Issuer:       {cert2.Issuer}\n" +
                   $"Valid from:   {cert2.NotBefore}\n" +
                   $"Valid until:  {cert2.NotAfter}\n" +
                   $"Thumbprint:   {cert2.Thumbprint}\n" +
                   $"Serial:       {cert2.SerialNumber}";
        }

        // Uses ip-api.com (free, no key) for geolocation
        public static async Task<string> IpGeolocationAsync(string ip)
        {
            var json = await Http.GetStringAsync($"http://ip-api.com/json/{ip}");
            return JsonFormat.Pretty(json);
        }

        public static async Task<string> AsnLookupAsync(string ip)
        {
            // ip-api.com's 'as' field returns the ASN + org in one string, no key needed
            var json = await Http.GetStringAsync($"http://ip-api.com/json/{ip}?fields=status,message,query,as,isp,org,country");
            return JsonFormat.Pretty(json);
        }

        public static async Task<string> RobotsTxtAsync(string host)
        {
            if (!host.StartsWith("http://") && !host.StartsWith("https://"))
                host = "https://" + host;
            try
            {
                return await Http.GetStringAsync(host.TrimEnd('/') + "/robots.txt");
            }
            catch (Exception ex)
            {
                return $"Failed to fetch robots.txt: {ex.Message}";
            }
        }

        public static async Task<string> WaybackCheckAsync(string url)
        {
            try
            {
                var json = await Http.GetStringAsync($"https://archive.org/wayback/available?url={Uri.EscapeDataString(url)}");
                return JsonFormat.Pretty(json);
            }
            catch (Exception ex)
            {
                return $"Wayback lookup failed: {ex.Message}";
            }
        }

        public static async Task<string> ReverseIpAsync(string ip)
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(ip);
                return entry.HostName;
            }
            catch (Exception ex)
            {
                return $"No PTR record found: {ex.Message}";
            }
        }

        // Uses the HaveIBeenPwned v3 API. Requires a paid API key (https://haveibeenpwned.com/API/Key).
        // The key is passed in from the UI and persisted via SettingsService so it only needs to be entered once.
        public static async Task<string> BreachCheckAsync(string email, string? apiKey = null)
        {
            apiKey = string.IsNullOrWhiteSpace(apiKey) ? SettingsService.Current.HibpApiKey : apiKey;

            if (string.IsNullOrWhiteSpace(apiKey))
                return "Breach checking requires a HaveIBeenPwned API key.\n" +
                       "Enter your key above and click Lookup - it will be saved locally for next time.\n" +
                       "Get a key at https://haveibeenpwned.com/API/Key";

            if (string.IsNullOrWhiteSpace(email))
                return "Enter an email address to check.";

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://haveibeenpwned.com/api/v3/breachedaccount/{Uri.EscapeDataString(email)}?truncateResponse=false");
            request.Headers.Add("hibp-api-key", apiKey);
            request.Headers.Add("User-Agent", "RoseNg-OSINT-Tool");

            try
            {
                using var resp = await Http.SendAsync(request);

                if (resp.StatusCode == HttpStatusCode.NotFound)
                    return $"No breaches found for {email}.";
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                    return "HaveIBeenPwned rejected the API key (invalid or expired).";
                if ((int)resp.StatusCode == 429)
                    return "Rate limited by HaveIBeenPwned. Wait a bit and try again.";
                if (!resp.IsSuccessStatusCode)
                    return $"Breach check failed: {(int)resp.StatusCode} {resp.StatusCode}";

                var json = await resp.Content.ReadAsStringAsync();
                return JsonFormat.Pretty(json);
            }
            catch (Exception ex)
            {
                return $"Breach check failed: {ex.Message}";
            }
        }

        public static async Task<string> UsernameSearchAsync(string username)
        {
            var sites = new (string Name, string UrlTemplate)[]
            {
                ("GitHub", "https://github.com/{0}"),
                ("Twitter/X", "https://x.com/{0}"),
                ("Instagram", "https://instagram.com/{0}"),
                ("Reddit", "https://reddit.com/user/{0}"),
                ("TikTok", "https://tiktok.com/@{0}"),
            };

            var sb = new StringBuilder();
            foreach (var (name, template) in sites)
            {
                var url = string.Format(template, username);
                try
                {
                    var resp = await Http.GetAsync(url);
                    sb.AppendLine($"{name,-12} {(resp.IsSuccessStatusCode ? "FOUND" : "not found")}  {url}");
                }
                catch
                {
                    sb.AppendLine($"{name,-12} error checking  {url}");
                }
            }
            return sb.ToString();
        }
    }
}
