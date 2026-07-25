namespace RoseNG.Core.Discord;

public static class SnowflakeTool
{
    // Discord epoch: 2015-01-01T00:00:00.000Z
    private const long DiscordEpochMs = 1420070400000L;

    public static ToolResult Decode(string snowflakeText)
    {
        if (!ulong.TryParse(snowflakeText.Trim(), out var snowflake))
            return ToolResult.Fail("Enter a valid numeric Discord snowflake ID.");

        var timestampMs = (long)(snowflake >> 22) + DiscordEpochMs;
        var workerId = (snowflake & 0x3E0000) >> 17;
        var processId = (snowflake & 0x1F000) >> 12;
        var increment = snowflake & 0xFFF;

        var created = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);

        var output =
            $"Timestamp (UTC): {created:yyyy-MM-dd HH:mm:ss} UTC\n" +
            $"Unix (seconds):  {created.ToUnixTimeSeconds()}\n" +
            $"Internal worker: {workerId}\n" +
            $"Internal process:{processId}\n" +
            $"Increment:       {increment}";

        return ToolResult.Ok(output, new Dictionary<string, string>
        {
            ["UnixSeconds"] = created.ToUnixTimeSeconds().ToString()
        });
    }

    /// <summary>Builds a Discord `&lt;t:UNIX:FORMAT&gt;` markdown timestamp for a given local DateTime.</summary>
    public static ToolResult BuildTimestamp(DateTime local, char format = 'f')
    {
        var unix = new DateTimeOffset(local.ToUniversalTime()).ToUnixTimeSeconds();
        return ToolResult.Ok($"<t:{unix}:{format}>");
    }
}
