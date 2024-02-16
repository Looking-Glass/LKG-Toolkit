using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using System.IO;
using ToolkitGUI.Media;

namespace ToolkitGUI
{
    public partial class PlaylistItemControl : UserControl
    {
        public PlaylistControl parent;
        public int index;
        public PlaylistItem item;
        private bool isSelected;
        private bool isPlaying;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
            }
        }
        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                isPlaying = value;
            }
        }

        public PlaylistItemControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            FileNameTextBlock = this.FindControl<TextBlock>("FileNameTextBlock");
            MediaTypeTextBlock = this.FindControl<TextBlock>("MediaTypeTextBlock");
            SelectionBorder = this.FindControl<Border>("SelectionBorder");
            DeleteButton = this.FindControl<Button>("DeleteButton");
            PreviewImage = this.FindControl<Image>("PreviewImage");

            DeleteButton.IsVisible = false;
        }

        public void SetPlaylistItem(PlaylistControl parent, PlaylistItem item, int index)
        {
            this.parent = parent;
            this.item = item;
            this.index = index;

            FileNameTextBlock.Text = Path.GetFileName(item.path);
            MediaTypeTextBlock.Text = item.mtype.ToString();
            //if(item.PreviewBitmap != null )
            //{
            //    PreviewImage.Source = item.PreviewBitmap;
            //}

            IsSelected = false;
            SetItemPlaying(false);
        }

        public void SetItemSelected(bool selected)
        {
            IsSelected = selected;
            DeleteButton.IsVisible = selected;

            if (IsSelected)
            {
                SelectionBorder.Background = new SolidColorBrush(Colors.LightGray);
            }
            else
            {
                SelectionBorder.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        public void SetItemPlaying(bool playing)
        {
            isPlaying = playing;

            if (isPlaying)
            {
                SelectionBorder.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                SelectionBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
        }

        // Handler for the Delete button click event
        private void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            parent.DeleteItem(index);
        }
    }
}
