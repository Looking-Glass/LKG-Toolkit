using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ToolkitGUI.Media;

namespace ToolkitGUI
{
    public partial class PlaylistControl : UserControl
    {
        public Playlist current;
        public int SelectedIndex { get; private set; } = -1;
        public string SelectedDrive = "";
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

            SyncLocationDropDown = this.FindControl<ComboBox>("SyncLocationDropDown");
            SyncLocationDropDown.SelectionChanged += SyncLocationDropDown_SelectionChanged;

            SyncPlaylistButton = this.FindControl<Button>("SyncPlaylistButton");
            SyncPlaylistButton.Click += SyncPlaylistButton_Click;

            var driveNames = new List<string>(); // Create a list to hold drive names

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name != "C:\\") // Check if the drive is ready to be accessed
                {
                    driveNames.Add(drive.Name); // Add the drive name to the list
                }
            }

            SyncLocationDropDown.Items = driveNames; // Assign the list to the Items property

            if (SyncLocationDropDown.ItemCount > 0)
            {
                SyncLocationDropDown.SelectedIndex = 0;
            }
        }

        private void SyncPlaylistButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if(SelectedDrive != "") 
            {
                Sync(SelectedDrive);
            }
        }

        private void SyncLocationDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SyncLocationDropDown.SelectedIndex != -1)
            {
                SelectedDrive = (string)SyncLocationDropDown.SelectedItem;
            }
        }

        private void Sync(string path)
        {
            Trace.WriteLine($"Syncing with drive: {path}");

            if (current != null)
            {
                // Define a list of allowed image file extensions
                var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };

                // Clean the drive
                try
                {
                    var filesInRoot = Directory.EnumerateFiles(path);
                    foreach (var file in filesInRoot)
                    {
                        string extension = Path.GetExtension(file);
                        if (allowedExtensions.Contains(extension) || extension.Equals(".m3u", StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error cleaning the drive: {ex.Message}");
                }

                // Filter out non-RGBD images that are of the allowed file types
                var nonRgbdImageItems = current.items
                    .Where(item => item.isRGBD == 0 && allowedExtensions.Contains(Path.GetExtension(item.path)))
                    .ToList();

                if (nonRgbdImageItems.Any())
                {
                    // Create the full path for the .m3u file
                    string m3uFilePath = Path.Combine(path, $"{current.name}.m3u");

                    // Copy files and write the .m3u file
                    using (StreamWriter writer = new StreamWriter(m3uFilePath, false))
                    {
                        foreach (var item in nonRgbdImageItems)
                        {
                            string fileName = Path.GetFileName(item.path);
                            string destinationPath = Path.Combine(path, fileName);

                            try
                            {
                                File.Copy(item.path, destinationPath, true); // Copy the file
                                writer.WriteLine(fileName); // Write the file name to the .m3u file
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine($"Error copying file {item.path}: {ex.Message}");
                            }
                        }
                    }

                    Trace.WriteLine($"Playlist synced to {m3uFilePath}");
                }
                else
                {
                    Trace.WriteLine("No suitable non-RGBD image files found in the current playlist.");
                }
            }
            else
            {
                Trace.WriteLine("No current playlist to sync.");
            }
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
