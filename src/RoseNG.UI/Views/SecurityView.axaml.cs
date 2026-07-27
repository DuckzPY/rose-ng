using Avalonia.Controls;
using RoseNG.Core.Services;
using System;

namespace RoseNG.UI.Views
{
    public partial class SecurityView : UserControl
    {
        public SecurityView()
        {
            InitializeComponent();
        }

        private string SelectedAlgo(ComboBox box) => (box.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "SHA256";

        private void GenPwBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            int.TryParse(PwLengthInput.Text, out var len);
            if (len <= 0) len = 16;
            PwOutput.Text = SecurityService.GeneratePassword(
                len,
                PwSymbolsCheck.IsChecked == true,
                PwDigitsCheck.IsChecked == true,
                PwUpperCheck.IsChecked == true);
        }

        private void StrengthBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => StrengthOutput.Text = SecurityService.CheckStrength(StrengthInput.Text ?? "");

        private void HashBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => HashOutput.Text = SecurityService.HashString(HashInput.Text ?? "", SelectedAlgo(HashAlgoBox));

        private void IdHashBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => IdHashOutput.Text = SecurityService.IdentifyHash(IdHashInput.Text ?? "");

        private void CrackBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var result = SecurityService.CrackHash(CrackHashInput.Text ?? "", SelectedAlgo(CrackAlgoBox), WordlistPathInput.Text ?? "");
            CrackOutput.Text = result ?? "Not found in wordlist (or wordlist path invalid)";
        }

        private void ChecksumBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                ChecksumOutput.Text = SecurityService.FileChecksum(FilePathInput.Text ?? "", SelectedAlgo(FileAlgoBox));
            }
            catch (Exception ex)
            {
                ChecksumOutput.Text = $"Error: {ex.Message}";
            }
        }
    }
}
