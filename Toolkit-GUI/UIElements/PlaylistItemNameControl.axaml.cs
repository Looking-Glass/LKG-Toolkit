using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using ToolkitGUI.Media;

namespace ToolkitGUI
{
    public partial class PlaylistItemNameControl : UserControl
    {
        public Action<Playlist> onPlaylistSelected;
        public Playlist item;

        public PlaylistItemNameControl()
        {
            InitializeComponent();
        }

        public PlaylistItemNameControl(Playlist item, Action<Playlist> onPlaylistSelected)
        {
            InitializeComponent();

            this.item = item;
            this.onPlaylistSelected = onPlaylistSelected;
            PlaylistNameTextBlock.Text = item.name;
            SelectButton.Click += SelectButton_Click;
        }

        private void SelectButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if(onPlaylistSelected != null)
            {
                onPlaylistSelected(item);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            PlaylistNameTextBlock = this.FindControl<TextBlock>("PlaylistNameTextBlock");
            SelectButton = this.FindControl<Button>("SelectButton");
        }
    }
}
