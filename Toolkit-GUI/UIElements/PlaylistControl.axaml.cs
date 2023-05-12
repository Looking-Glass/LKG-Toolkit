using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using ToolkitGUI.Media;

namespace ToolkitGUI
{
    public partial class PlaylistControl : UserControl
    {
        public Playlist current;
        public int SelectedIndex { get; private set; } = -1;
        public Action<Playlist, PlaylistItem> onSelectionChanged;

        public PlaylistControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            PlaylistHeader = this.FindControl<TextBlock>("PlaylistHeader");
            PlaylistItemsPanel = this.FindControl<StackPanel>("PlaylistItemsPanel");

            AddFileButton = this.FindControl<Button>("AddFileButton");
            AddFileButton.Click += AddFileButton_Click;

            AddRGBDFileButton = this.FindControl<Button>("AddRGBDFileButton");
            AddRGBDFileButton.Click += AddRGBDFileButton_Click;

            PlayPlaylistButton = this.FindControl<Button>("PlayPlaylistButton");
            PlayPlaylistButton.Click += PlayPlaylistButton_Click;
        }

        private void PlayPlaylistButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MainWindow.instance.bridgeConnection.TryPlayPlaylist(current.GetBridgePlaylist(), -1);
        }

        public void SetPlaylist(Playlist playlist)
        {
            current = playlist;
            PlaylistHeader.Text = playlist.name;

            UpdatePlaylistItems();
            UpdateSelectedItem(0);
        }

        public void UpdatePlaylistItems()
        {
            PlaylistItemsPanel.Children.Clear();

            int index = 0;
            
            foreach (var item in current.items)
            {
                var itemControl = new PlaylistItemControl();
                itemControl.SetPlaylistItem(item);
                int currentIndex = index;
                itemControl.PointerPressed += (s, e) =>
                {
                    UpdateSelectedItem(currentIndex);
                };
                PlaylistItemsPanel.Children.Add(itemControl);
                index++;
            }
        }

        private void UpdateSelectedItem(int index)
        {
            if (SelectedIndex >= 0 && SelectedIndex < PlaylistItemsPanel.Children.Count)
            {
                var previousSelected = (PlaylistItemControl)PlaylistItemsPanel.Children[SelectedIndex];
                previousSelected.IsSelected = false;
            }

            SelectedIndex = index;

            if(PlaylistItemsPanel.Children.Count > SelectedIndex)
            {
                var newSelected = (PlaylistItemControl)PlaylistItemsPanel.Children[SelectedIndex];
                newSelected.IsSelected = true;
            }

            if (onSelectionChanged != null)
            {
                if(current.items.Count > SelectedIndex)
                {
                    onSelectionChanged(current, current.items[SelectedIndex]);
                }
            }
        }

        private async void AddFileButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Title = "Add File to Playlist",
            };

            var result = await openFileDialog.ShowAsync((Window)this.VisualRoot);

            if (result != null && result.Length > 0)
            {
                foreach (var filename in result)
                {
                    current.AddFile(filename, false);
                }
            }

            PlaylistManager.Instance.SavePlaylist(current);
            UpdatePlaylistItems();
        }

        private async void AddRGBDFileButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Title = "Add RGBD File to Playlist",
            };

            var result = await openFileDialog.ShowAsync((Window)this.VisualRoot);

            if (result != null && result.Length > 0)
            {
                foreach (var filename in result)
                {
                    current.AddFile(filename, true);
                }
            }

            PlaylistManager.Instance.SavePlaylist(current);
            UpdatePlaylistItems();
        }
    }
}
