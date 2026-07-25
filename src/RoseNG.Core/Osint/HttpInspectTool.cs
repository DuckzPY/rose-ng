using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RoseNG.Core.Osint;

public static class HttpInspectTool
{
    public static async Task<ToolResult> GetHeadersAsync(string url, CancellationToken ct = default)
    {
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }

        try
        {
            using var handler = new HttpClientHandler { AllowAutoRedirect = false };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RoseNG/1.0 (+https://github.com/DuckzPY/rose-ng)");

            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);

            var sb = new StringBuilder();
            sb.AppendLine($"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}");
            sb.AppendLine();
            foreach (var h in response.Headers)
                sb.AppendLine($"{h.Key}: {string.Join(", ", h.Value)}");
            foreach (var h in response.Content.Headers)
                sb.AppendLine($"{h.Key}: {string.Join(", ", h.Value)}");

            return ToolResult.Ok(sb.ToString().TrimEnd());
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Request failed: {ex.Message}");
        }
    }

    public static async Task<ToolResult> GetTlsCertAsync(string host, int port = 443, CancellationToken ct = default)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port, ct);
            using var ssl = new SslStream(client.GetStream(), false, (_, _, _, _) => true);
            await ssl.AuthenticateAsClientAsync(host);

            if (ssl.RemoteCertificate is not X509Certificate2 cert)
            {
                // Upcast when needed (some runtimes hand back X509Certificate)
                var raw = ssl.RemoteCertificate;
                if (raw is null) return ToolResult.Fail("No certificate presented.");
                cert = new X509Certificate2(raw);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Subject:      {cert.Subject}");
            sb.AppendLine($"Issuer:       {cert.Issuer}");
            sb.AppendLine($"Valid from:   {cert.NotBefore:u}");
            sb.AppendLine($"Valid until:  {cert.NotAfter:u}");
            sb.AppendLine($"Thumbprint:   {cert.Thumbprint}");
            sb.AppendLine($"Serial:       {cert.SerialNumber}");

            return ToolResult.Ok(sb.ToString().TrimEnd());
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"TLS inspection failed: {ex.Message}");
        }
    }
}
