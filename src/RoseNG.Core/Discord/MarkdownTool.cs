namespace RoseNG.Core.Discord;

/// <summary>Small helpers that wrap text in Discord-flavoured markdown.</summary>
public static class MarkdownTool
{
    public static string Bold(string text) => $"**{text}**";
    public static string Italic(string text) => $"*{text}*";
    public static string Underline(string text) => $"__{text}__";
    public static string Strikethrough(string text) => $"~~{text}~~";
    public static string Spoiler(string text) => $"||{text}||";
    public static string InlineCode(string text) => $"`{text}`";
    public static string CodeBlock(string text, string language = "") => $"```{language}\n{text}\n```";
    public static string Quote(string text) =>
        string.Join('\n', text.Split('\n').Select(l => $"> {l}"));
    public static string Link(string label, string url) => $"[{label}]({url})";
}
