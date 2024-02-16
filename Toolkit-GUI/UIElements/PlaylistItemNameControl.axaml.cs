using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using ToolkitGUI.Media;

namespace ToolkitGUI
{
    public partial class PlaylistItemNameControl : UserControl
    {
        public Action<Playlist> onPlaylistSelected;
        public Action<Playlist> onDeletePlaylist;
        public Playlist item;

        public PlaylistItemNameControl()
        {
            InitializeComponent();
        }

        public PlaylistItemNameControl(Playlist item, Action<Playlist> onPlaylistSelected, Action<Playlist> onDeletePlaylist)
        {
            InitializeComponent();

            this.item = item;
            this.onPlaylistSelected = onPlaylistSelected;
            this.onDeletePlaylist = onDeletePlaylist;
            PlaylistNameTextBlock.Text = item.name;
            DeleteButton.Click += DeleteButton_Click;
        }

        private void Border_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (onPlaylistSelected != null)
            {
                onPlaylistSelected(item);
            }
        }

        private void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (onDeletePlaylist != null)
            {
                onDeletePlaylist(item);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            PlaylistNameTextBlock = this.FindControl<TextBlock>("PlaylistNameTextBlock");
            DeleteButton = this.FindControl<Button>("DeleteButton");
        }
    }
}
