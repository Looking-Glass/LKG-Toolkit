using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using ToolkitGUI.Media;

namespace ToolkitGUI
{
    public partial class PlaylistControl : UserControl
    {
        public Playlist current;
        public int SelectedIndex { get; private set; } = -1;
        public int PlayingIndex { get; private set; } = -1;
        public Action<Playlist, PlaylistItem> onSelectionChanged;
        private bool isPlaying = false;
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
            if (!isPlaying)
            {
                MainWindow.instance.PlayingPlaylist = current;
                MainWindow.instance.bridgeConnection.AddListener("New Item Playing", (string data) =>
                {
                    try
                    {
                        var json = System.Text.Json.JsonDocument.Parse(data);
                        var playlistName = json.RootElement.GetProperty("playlist_name").GetProperty("value").GetString();
                        var itemIndex = json.RootElement.GetProperty("index").GetProperty("value").GetInt32();

                        if(current.name == playlistName)
                        {
                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                UpdatePlayingItem(itemIndex);
                            }).Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Failed to parse JSON data: {ex.Message}");
                    }
                });
            }
            isPlaying = true;

            MainWindow.instance.bridgeConnection.TryPlayPlaylist(current.GetBridgePlaylist(), -1);
        }

        public void SetPlaylist(Playlist playlist)
        {
            current = playlist;
            PlaylistHeader.Text = playlist.name;

            UpdatePlaylistItems();
            UpdateSelectedItem(0);
        }

        public void DeleteItem(int index)
        {
            if (current != null && index >= 0 && index < current.items.Count)
            {
                current.RemovePlaylistItem(index);
                SetPlaylist(current);
            }
        }

        public void UpdatePlaylistItems()
        {
            PlaylistItemsPanel.Children.Clear();

            int index = 0;
            
            foreach (var item in current.items)
            {
                var itemControl = new PlaylistItemControl();
                itemControl.SetPlaylistItem(this, item, index);
                int currentIndex = index;
                itemControl.PointerPressed += (s, e) =>
                {
                    UpdateSelectedItem(currentIndex);
                };
                PlaylistItemsPanel.Children.Add(itemControl);
                index++;
            }
        }

        private void UpdatePlayingItem(int index)
        {
            if (PlayingIndex >= 0 && PlayingIndex < PlaylistItemsPanel.Children.Count)
            {
                var previousSelected = (PlaylistItemControl)PlaylistItemsPanel.Children[PlayingIndex];
                previousSelected.SetItemPlaying(false);
            }

            PlayingIndex = index;

            if (PlaylistItemsPanel.Children.Count > PlayingIndex)
            {
                var newSelected = (PlaylistItemControl)PlaylistItemsPanel.Children[PlayingIndex];
                newSelected.SetItemPlaying(true);
            }
        }

        private void UpdateSelectedItem(int index)
        {
            if (SelectedIndex >= 0 && SelectedIndex < PlaylistItemsPanel.Children.Count)
            {
                var previousSelected = (PlaylistItemControl)PlaylistItemsPanel.Children[SelectedIndex];
                previousSelected.SetItemSelected(false);
            }

            SelectedIndex = index;

            if(PlaylistItemsPanel.Children.Count > SelectedIndex)
            {
                var newSelected = (PlaylistItemControl)PlaylistItemsPanel.Children[SelectedIndex];
                newSelected.SetItemSelected(true);
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
