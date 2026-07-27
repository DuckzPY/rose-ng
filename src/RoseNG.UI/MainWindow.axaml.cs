using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using RoseNG.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoseNG.UI
{
    public partial class MainWindow : Window
    {
        private const double Gap = 14;

        public MainWindow()
        {
            InitializeComponent();
            ShowHome();
        }

        private void SearchBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            var text = SearchBox.Text ?? "";

            if (string.IsNullOrWhiteSpace(text))
            {
                SearchPopup.IsOpen = false;
                return;
            }

            // If the pasted/typed text looks like a real target (IP, hash, JWT,
            // domain, webhook URL, etc.) put that suggestion first.
            var results = new List<RoseNG.Core.SearchItem>();
            var detected = RoseNG.Core.ToolIndex.DetectTarget(text);
            if (detected != null) results.Add(detected);
            results.AddRange(RoseNG.Core.ToolIndex.Search(text).Where(r => detected == null || r.Tool != detected.Tool));

            if (results.Count == 0)
            {
                SearchPopup.IsOpen = false;
                return;
            }

            SearchResults.ItemsSource = results;
            SearchPopup.IsOpen = true;
        }

        private void SearchBox_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Small delay so a click on a result item still registers before we close
            Avalonia.Threading.Dispatcher.UIThread.Post(() => SearchPopup.IsOpen = false,
                Avalonia.Threading.DispatcherPriority.Background);
        }

        private void SearchResultBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button { Tag: RoseNG.Core.SearchItem item })
            {
                SearchPopup.IsOpen = false;
                SearchBox.Text = "";
                GoToCategory(item.Category);
            }
        }

        private void GoToCategory(string category)
        {
            switch (category)
            {
                case "OSINT": NavigateTo(new OsintView()); break;
                case "Network": NavigateTo(new NetworkView()); break;
                case "Discord": NavigateTo(new DiscordView()); break;
                case "Security": NavigateTo(new SecurityView()); break;
                case "Encoding": NavigateTo(new EncodingView()); break;
            }
        }

        private void SearchBox_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var text = SearchBox.Text?.Trim() ?? "";
            if (text.Length == 0) return;

            SearchPopup.IsOpen = false;

            var detected = RoseNG.Core.ToolIndex.DetectTarget(text);
            if (detected != null) { GoToCategory(detected.Category); return; }

            var results = RoseNG.Core.ToolIndex.Search(text);
            if (results.Count > 0) GoToCategory(results[0].Category);
        }

        public void ShowHome() => ContentHost.Content = BuildHome();

        public void NavigateTo(UserControl view)
        {
            var backBtn = new Button
            {
                Content = "\u2190  back",
                Classes = { "backBtn" },
                Margin = new Avalonia.Thickness(0, 0, 0, 20),
                Focusable = false
            };
            backBtn.Click += (_, _) => ShowHome();

            var stack = new StackPanel
            {
                Spacing = 4,
                Opacity = 0,
                Transitions = new Avalonia.Animation.Transitions
                {
                    new Avalonia.Animation.DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(180) }
                }
            };
            stack.Children.Add(backBtn);
            stack.Children.Add(view);

            ContentHost.Content = new ScrollViewer { Content = stack };

            // Kick the fade off after layout so it actually animates in
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                stack.Opacity = 1;
            }, Avalonia.Threading.DispatcherPriority.Loaded);
        }

        private Control BuildHome()
        {
            var root = new Grid
            {
                RowDefinitions = new RowDefinitions($"*,{Gap},Auto"),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Top: featured OSINT (left, spans both rows) + Network/Discord (right column)
            var topGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions($"1.3*,{Gap},1*"),
                RowDefinitions = new RowDefinitions($"*,{Gap},*"),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Grid.SetRow(topGrid, 0);

            var osintBtn = MakeFeaturedTile("OSINT", "WHOIS · DNS · SSL · subnet calc · ASN · robots.txt · Wayback · username search",
                () => NavigateTo(new OsintView()));
            Grid.SetColumn(osintBtn, 0);
            Grid.SetRow(osintBtn, 0);
            Grid.SetRowSpan(osintBtn, 3);

            var networkBtn = MakeTile("Network", "port scan · ping · traceroute · headers · WoL",
                () => NavigateTo(new NetworkView()));
            Grid.SetColumn(networkBtn, 2);
            Grid.SetRow(networkBtn, 0);

            var discordBtn = MakeTile("Discord", "webhooks · snowflake · emojis · roles · audit log",
                () => NavigateTo(new DiscordView()));
            Grid.SetColumn(discordBtn, 2);
            Grid.SetRow(discordBtn, 2);

            topGrid.Children.Add(osintBtn);
            topGrid.Children.Add(networkBtn);
            topGrid.Children.Add(discordBtn);

            // Bottom: Security (under OSINT) / Encoding (under Network+Discord) - same
            // column ratio as the top grid so every tile edge lines up vertically
            var bottomGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions($"1.3*,{Gap},1*"),
                Height = 150,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            Grid.SetRow(bottomGrid, 2);

            var securityBtn = MakeTile("Security", "password gen · hash id/crack · file integrity",
                () => NavigateTo(new SecurityView()));
            Grid.SetColumn(securityBtn, 0);

            var encodingBtn = MakeTile("Encoding", "base64 · hex · JWT · URL · ROT13 · XOR",
                () => NavigateTo(new EncodingView()));
            Grid.SetColumn(encodingBtn, 2);

            bottomGrid.Children.Add(securityBtn);
            bottomGrid.Children.Add(encodingBtn);

            root.Children.Add(topGrid);
            root.Children.Add(bottomGrid);
            return root;
        }

        private Button MakeTile(string title, string subtitle, Action onClick)
        {
            var btn = new Button
            {
                Classes = { "tile" },
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Focusable = false
            };
            var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, Spacing = 6 };
            stack.Children.Add(new TextBlock { Text = title, Classes = { "tileTitle" } });
            stack.Children.Add(new TextBlock { Text = subtitle, Classes = { "tileSubtitle" }, TextWrapping = TextWrapping.Wrap });
            btn.Content = stack;
            btn.Click += (_, _) => onClick();
            return btn;
        }

        private Button MakeFeaturedTile(string title, string subtitle, Action onClick)
        {
            var btn = new Button
            {
                Classes = { "featured" },
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Focusable = false
            };
            var grid = new Grid { RowDefinitions = new RowDefinitions("Auto,*,Auto") };

            var badge = new Border { Classes = { "badge" }, HorizontalAlignment = HorizontalAlignment.Right };
            badge.Child = new TextBlock { Text = "most used", FontSize = 12, Foreground = new SolidColorBrush(Color.Parse("#F0A3AE")) };
            Grid.SetRow(badge, 0);

            var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, Spacing = 8 };
            stack.Children.Add(new TextBlock { Text = title, FontSize = 27, FontWeight = FontWeight.SemiBold, Foreground = Brushes.White });
            stack.Children.Add(new TextBlock { Text = subtitle, Classes = { "tileSubtitle" }, TextWrapping = TextWrapping.Wrap });
            Grid.SetRow(stack, 2);

            grid.Children.Add(badge);
            grid.Children.Add(stack);
            btn.Content = grid;
            btn.Click += (_, _) => onClick();
            return btn;
        }
    }
}
