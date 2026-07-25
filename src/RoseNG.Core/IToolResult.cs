namespace RoseNG.Core;

/// <summary>
/// Uniform result envelope returned by every backend tool call.
/// Keeping this consistent is what lets the UI render any tool
/// (present + future) with one generic result view.
/// </summary>
public sealed class ToolResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string Output { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string>? Fields { get; init; }

    public static ToolResult Ok(string output, IReadOnlyDictionary<string, string>? fields = null)
        => new() { Success = true, Output = output, Fields = fields };

    public static ToolResult Fail(string error)
        => new() { Success = false, Error = error, Output = string.Empty };
}
