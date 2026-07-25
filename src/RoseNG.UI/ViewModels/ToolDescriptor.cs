using RoseNG.Core;

namespace RoseNG.UI.ViewModels;

/// <summary>
/// Describes one tool in the catalog: what it's called, which category it
/// lives under, what keywords the search bar should match, and the single
/// async delegate that runs it. New tools are added by appending one entry
/// to ToolCatalog.All - no other UI code needs to change.
/// </summary>
public sealed class ToolDescriptor
{
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required string Description { get; init; }
    public string[] Keywords { get; init; } = Array.Empty<string>();
    public string InputPlaceholder { get; init; } = "Enter input...";
    public required Func<string, CancellationToken, Task<ToolResult>> Execute { get; init; }

    public bool Matches(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return true;
        query = query.Trim();
        return Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || Category.Contains(query, StringComparison.OrdinalIgnoreCase)
            || Description.Contains(query, StringComparison.OrdinalIgnoreCase)
            || Keywords.Any(k => k.Contains(query, StringComparison.OrdinalIgnoreCase));
    }
}
