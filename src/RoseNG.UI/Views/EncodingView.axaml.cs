using Avalonia.Controls;
using RoseNG.Core.Services;

namespace RoseNG.UI.Views
{
    public partial class EncodingView : UserControl
    {
        public EncodingView()
        {
            InitializeComponent();
        }

        private void B64EncodeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => B64Output.Text = EncodingService.Base64Encode(B64Input.Text ?? "");

        private void B64DecodeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => B64Output.Text = EncodingService.Base64Decode(B64Input.Text ?? "");

        private void HexEncodeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => HexOutput.Text = EncodingService.HexEncode(HexInput.Text ?? "");

        private void HexDecodeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => HexOutput.Text = EncodingService.HexDecode(HexInput.Text ?? "");

        private void JwtBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => JwtOutput.Text = EncodingService.DecodeJwt(JwtInput.Text ?? "");

        private void UrlEncodeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => UrlOutput.Text = EncodingService.UrlEncode(UrlInput.Text ?? "");

        private void UrlDecodeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => UrlOutput.Text = EncodingService.UrlDecode(UrlInput.Text ?? "");

        private void Rot13Btn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => Rot13Output.Text = EncodingService.Rot13(Rot13Input.Text ?? "");

        private void XorEncryptBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => XorOutput.Text = EncodingService.XorCipher(XorInput.Text ?? "", XorKeyInput.Text ?? "");

        private void XorDecryptBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => XorOutput.Text = EncodingService.XorDecipherHex(XorInput.Text ?? "", XorKeyInput.Text ?? "");
    }
}
