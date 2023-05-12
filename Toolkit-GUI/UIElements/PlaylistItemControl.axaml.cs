using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.IO;
using ToolkitGUI.Media;

namespace ToolkitGUI
{
    public partial class PlaylistItemControl : UserControl
    {
        public PlaylistItem item;
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;

                if (isSelected)
                {
                    SelectionBorder.Background = new SolidColorBrush(Colors.LightGray);
                }
                else
                {
                    SelectionBorder.Background = new SolidColorBrush(Colors.Transparent);
                }
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
        }

        public void SetPlaylistItem(PlaylistItem item)
        {
            this.item = item;

            FileNameTextBlock.Text = System.IO.Path.GetFileName(item.path);
            MediaTypeTextBlock.Text = item.mtype.ToString();
            SelectionBorder = this.FindControl<Border>("SelectionBorder");

            IsSelected = false;
        }

    }
}