using Avalonia.Controls;
using RoseNG.Core.Services;

namespace RoseNG.UI.Views
{
    public partial class DiscordView : UserControl
    {
        public DiscordView()
        {
            InitializeComponent();
        }

        private async void SendWebhookBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => WebhookOutput.Text = await DiscordService.SendWebhookAsync(WebhookUrlInput.Text ?? "", WebhookMsgInput.Text ?? "");

        private async void SendEmbedBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => EmbedOutput.Text = await DiscordService.SendEmbedAsync(
                EmbedWebhookInput.Text ?? "", EmbedTitleInput.Text ?? "", EmbedDescInput.Text ?? "", EmbedColorInput.Text ?? "#5865F2");

        private void SnowflakeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => SnowflakeOutput.Text = DiscordService.DecodeSnowflake(SnowflakeInput.Text ?? "");

        private void TokenFormatBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => TokenOutput.Text = DiscordService.ValidateTokenFormat(TokenInput.Text ?? "");

        private async void TokenCheckBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => TokenOutput.Text = await DiscordService.CheckOwnBotTokenAsync(TokenInput.Text ?? "");

        private async void InviteBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => InviteOutput.Text = await DiscordService.ResolveInviteAsync(InviteInput.Text ?? "");

        private async void AuditBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => AuditOutput.Text = await DiscordService.GetOwnGuildAuditLogAsync(AuditGuildInput.Text ?? "", AuditTokenInput.Text ?? "");

        private async void WhInfoBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => WhInfoOutput.Text = await DiscordService.GetWebhookInfoAsync(WhInfoUrlInput.Text ?? "");

        private async void EmojiBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => EmojiOutput.Text = await DiscordService.GetOwnGuildEmojisAsync(EmojiGuildInput.Text ?? "", EmojiTokenInput.Text ?? "");

        private async void RoleBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => RoleOutput.Text = await DiscordService.GetOwnGuildRolesAsync(RoleGuildInput.Text ?? "", RoleTokenInput.Text ?? "");
    }
}
