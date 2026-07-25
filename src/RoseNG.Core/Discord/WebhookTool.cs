using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RoseNG.Core.Discord;

public sealed class DiscordEmbed
{
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("color")] public int? Color { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("footer")] public DiscordEmbedFooter? Footer { get; set; }
    [JsonPropertyName("timestamp")] public string? Timestamp { get; set; }
}

public sealed class DiscordEmbedFooter
{
    [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
}

public sealed class WebhookPayload
{
    [JsonPropertyName("content")] public string? Content { get; set; }
    [JsonPropertyName("username")] public string? Username { get; set; }
    [JsonPropertyName("avatar_url")] public string? AvatarUrl { get; set; }
    [JsonPropertyName("embeds")] public List<DiscordEmbed>? Embeds { get; set; }
}

/// <summary>
/// Sends messages to Discord webhooks that belong to the user. This never
/// discovers, brute-forces, or scrapes webhook URLs - it only calls a URL
/// the user has explicitly pasted in, consistent with Discord's ToS.
/// </summary>
public static class WebhookTool
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(10) };

    public static async Task<ToolResult> TestAsync(string webhookUrl, CancellationToken ct = default)
    {
        if (!IsLikelyDiscordWebhook(webhookUrl))
            return ToolResult.Fail("That doesn't look like a Discord webhook URL.");

        try
        {
            var response = await Client.GetAsync(webhookUrl, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            return response.IsSuccessStatusCode
                ? ToolResult.Ok($"Webhook is valid.\n{body}")
                : ToolResult.Fail($"Webhook check failed: {(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Could not reach webhook: {ex.Message}");
        }
    }

    public static async Task<ToolResult> SendAsync(string webhookUrl, WebhookPayload payload, CancellationToken ct = default)
    {
        if (!IsLikelyDiscordWebhook(webhookUrl))
            return ToolResult.Fail("That doesn't look like a Discord webhook URL.");

        try
        {
            var response = await Client.PostAsJsonAsync(webhookUrl, payload, ct);
            return response.IsSuccessStatusCode
                ? ToolResult.Ok("Message sent successfully.")
                : ToolResult.Fail($"Discord returned {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(ct)}");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Send failed: {ex.Message}");
        }
    }

    private static bool IsLikelyDiscordWebhook(string url) =>
        url.StartsWith("https://discord.com/api/webhooks/", StringComparison.OrdinalIgnoreCase) ||
        url.StartsWith("https://discordapp.com/api/webhooks/", StringComparison.OrdinalIgnoreCase);
}
