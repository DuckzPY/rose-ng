using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoseNG.Core.Security;

namespace RoseNG.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public string DiscordInviteUrl => Links.DiscordInvite;

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ToolDescriptor? _selectedTool;
    [ObservableProperty] private string _toolInput = string.Empty;
    [ObservableProperty] private string _output = string.Empty;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _lastRunSucceeded;

    public ObservableCollection<ToolDescriptor> FilteredTools { get; } = new();

    public MainWindowViewModel()
    {
        RefreshFilter();
    }

    partial void OnSearchTextChanged(string value) => RefreshFilter();

    partial void OnSelectedToolChanged(ToolDescriptor? value)
    {
        Output = string.Empty;
        ToolInput = string.Empty;
    }

    private void RefreshFilter()
    {
        FilteredTools.Clear();
        foreach (var tool in ToolCatalog.All.Where(t => t.Matches(SearchText)))
            FilteredTools.Add(tool);
    }

    [RelayCommand]
    private async Task RunToolAsync()
    {
        if (SelectedTool is null || IsRunning) return;

        IsRunning = true;
        Output = "Running...";
        try
        {
            var result = await SelectedTool.Execute(ToolInput, CancellationToken.None);
            LastRunSucceeded = result.Success;
            Output = result.Success ? result.Output : $"Error: {result.Error}";
        }
        catch (Exception ex)
        {
            LastRunSucceeded = false;
            Output = $"Unexpected error: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
    }
}
