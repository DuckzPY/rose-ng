using Avalonia.Controls;
using RoseNG.Core.Services;
using System;

namespace RoseNG.UI.Views
{
    public partial class NetworkView : UserControl
    {
        public NetworkView()
        {
            InitializeComponent();
        }

        private async void PingBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => PingOutput.Text = await NetworkService.PingAsync(PingInput.Text ?? "");

        private async void TraceBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => TraceOutput.Text = await NetworkService.TracerouteAsync(TraceInput.Text ?? "");

        private async void ScanBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            int.TryParse(ScanStartInput.Text, out var start);
            int.TryParse(ScanEndInput.Text, out var end);
            ScanOutput.Text = await NetworkService.PortScanAsync(ScanHostInput.Text ?? "", start, end);
        }

        private async void ArpBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ArpOutput.Text = "Sweeping local subnet, this can take a few seconds...";
            ArpOutput.Text = await NetworkService.ArpSweepAsync();
        }

        private void MacBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => MacOutput.Text = NetworkService.MacVendorLookup(MacInput.Text ?? "");

        private async void HeaderBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => HeaderOutput.Text = await NetworkService.HttpHeaderGrabAsync(HeaderUrlInput.Text ?? "");

        private async void WolBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                await NetworkService.SendWakeOnLanAsync(WolInput.Text ?? "");
                WolOutput.Text = "Magic packet sent.";
            }
            catch (Exception ex)
            {
                WolOutput.Text = $"Failed: {ex.Message}";
            }
        }

        private void IfaceBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => IfaceOutput.Text = NetworkService.LocalInterfaceInfo();
    }
}
