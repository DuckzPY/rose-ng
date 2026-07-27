using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoseNG.Core.Services
{
    public static class DiscordService
    {
        private static readonly HttpClient Http = new HttpClient();
        private const long DiscordEpoch = 1420070400000L; // ms

        public static async Task<string> SendWebhookAsync(string webhookUrl, string content)
        {
            var payload = JsonSerializer.Serialize(new { content });
            var resp = await Http.PostAsync(webhookUrl,
                new StringContent(payload, Encoding.UTF8, "application/json"));
            return $"{(int)resp.StatusCode} {resp.StatusCode}";
        }

        public static async Task<string> SendEmbedAsync(string webhookUrl, string title, string description, string colorHex)
        {
            int color = Convert.ToInt32(colorHex.TrimStart('#'), 16);
            var payload = JsonSerializer.Serialize(new
            {
                embeds = new[]
                {
                    new { title, description, color }
                }
            });
            var resp = await Http.PostAsync(webhookUrl,
                new StringContent(payload, Encoding.UTF8, "application/json"));
            return $"{(int)resp.StatusCode} {resp.StatusCode}";
        }

        public static string DecodeSnowflake(string snowflakeId)
        {
            if (!ulong.TryParse(snowflakeId, out var id))
                return "Invalid snowflake ID";

            var timestampMs = (id >> 22) + (ulong)DiscordEpoch;
            var dt = DateTimeOffset.FromUnixTimeMilliseconds((long)timestampMs);
            var workerId = (id & 0x3E0000) >> 17;
            var processId = (id & 0x1F000) >> 12;
            var increment = id & 0xFFF;

            return $"Timestamp:  {dt:yyyy-MM-dd HH:mm:ss} UTC\n" +
                   $"Worker ID:  {workerId}\n" +
                   $"Process ID: {processId}\n" +
                   $"Increment:  {increment}";
        }

        // Format-level validation only. Actually verifying a token requires calling
        // Discord's /users/@me with it, which this app should only do against the
        // user's own bot token, never someone else's.
        public static string ValidateTokenFormat(string token)
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return "Not a valid Discord token format (expected 3 dot-separated segments).";
            return "Token has a structurally valid format (segments: id.timestamp.hmac). " +
                   "Use 'check own bot' to verify it against the API.";
        }

        public static async Task<string> CheckOwnBotTokenAsync(string token)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/v10/users/@me");
            req.Headers.Add("Authorization", $"Bot {token}");
            var resp = await Http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            return $"{(int)resp.StatusCode} {resp.StatusCode}\n\n{JsonFormat.Pretty(body)}";
        }

        public static async Task<string> GetWebhookInfoAsync(string webhookUrl)
        {
            var resp = await Http.GetAsync(webhookUrl);
            var body = await resp.Content.ReadAsStringAsync();
            return JsonFormat.Pretty(body);
        }

        public static async Task<string> GetOwnGuildEmojisAsync(string guildId, string botToken)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://discord.com/api/v10/guilds/{guildId}/emojis");
            req.Headers.Add("Authorization", $"Bot {botToken}");
            var resp = await Http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            return JsonFormat.Pretty(body);
        }

        public static async Task<string> GetOwnGuildRolesAsync(string guildId, string botToken)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://discord.com/api/v10/guilds/{guildId}/roles");
            req.Headers.Add("Authorization", $"Bot {botToken}");
            var resp = await Http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            return JsonFormat.Pretty(body);
        }

        public static async Task<string> ResolveInviteAsync(string inviteCode)
        {
            var resp = await Http.GetAsync($"https://discord.com/api/v10/invites/{inviteCode}?with_counts=true");
            var body = await resp.Content.ReadAsStringAsync();
            return JsonFormat.Pretty(body);
        }

        public static async Task<string> GetOwnGuildAuditLogAsync(string guildId, string botToken)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://discord.com/api/v10/guilds/{guildId}/audit-logs");
            req.Headers.Add("Authorization", $"Bot {botToken}");
            var resp = await Http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            return JsonFormat.Pretty(body);
        }
    }
}
