using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using RoseNG.UI.ViewModels;

namespace RoseNG.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnDiscordLinkClicked(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = vm.DiscordInviteUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            // Non-fatal - user's OS may lack a default handler; fail silently
            // rather than crash the app over an optional support link.
        }
    }
}
