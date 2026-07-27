using Avalonia.Controls;
using RoseNG.Core.Services;

namespace RoseNG.UI.Views
{
    public partial class OsintView : UserControl
    {
        public OsintView()
        {
            InitializeComponent();
            BreachApiKeyInput.Text = RoseNG.Core.Services.SettingsService.Current.HibpApiKey;
        }

        private async void WhoisBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => WhoisOutput.Text = await OsintService.WhoisAsync(WhoisInput.Text ?? "");

        private async void DnsBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => DnsOutput.Text = await OsintService.DnsLookupAsync(DnsInput.Text ?? "");

        private async void SslBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => SslOutput.Text = await OsintService.SslInspectAsync(SslInput.Text ?? "");

        private void CidrBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => CidrOutput.Text = OsintService.SubnetCalc(CidrInput.Text ?? "");

        private async void RevIpBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => RevIpOutput.Text = await OsintService.ReverseIpAsync(RevIpInput.Text ?? "");

        private async void GeoBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => GeoOutput.Text = await OsintService.IpGeolocationAsync(GeoInput.Text ?? "");

        private async void UserBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => UserOutput.Text = await OsintService.UsernameSearchAsync(UserInput.Text ?? "");

        private async void AsnBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => AsnOutput.Text = await OsintService.AsnLookupAsync(AsnInput.Text ?? "");

        private async void RobotsBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => RobotsOutput.Text = await OsintService.RobotsTxtAsync(RobotsInput.Text ?? "");

        private async void WaybackBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => WaybackOutput.Text = await OsintService.WaybackCheckAsync(WaybackInput.Text ?? "");

        private async void BreachBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var key = BreachApiKeyInput.Text ?? "";
            if (!string.IsNullOrWhiteSpace(key))
                RoseNG.Core.Services.SettingsService.SetHibpApiKey(key);

            BreachOutput.Text = await OsintService.BreachCheckAsync(BreachEmailInput.Text ?? "", key);
        }
    }
}
