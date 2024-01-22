using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using ToolkitAPI.Bridge.Params;
using ToolkitGUI.Media;

namespace ToolkitGUI
{
    public partial class PropertiesPaneControl : UserControl
    {
        Dictionary<int, int> _current_values = new Dictionary<int, int>();
        int font_size = 14;

        public PropertiesPaneControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            PropertiesStackPanel = this.FindControl<StackPanel>("PropertiesStackPanel");
            PropertiesBelowCollapseStackPanel = this.FindControl<StackPanel>("PropertiesBelowCollapseStackPanel");
        }


        private void AddIntegerInput(string label, int initialValue, Action<int> onUpdate)
        {
            var labelControl = new TextBlock 
            { 
                Text = label,
                FontSize = font_size,
                Margin = new Thickness(3)
            };
            currentPanelToWriteTo.Children.Add(labelControl);

            var inputControl = new NumericUpDown { Value = initialValue };
            inputControl.FontSize = font_size;
            inputControl.ValueChanged += (sender, e) => onUpdate((int)e.NewValue);
            currentPanelToWriteTo.Children.Add(inputControl);
        }

        private void AddBoolInput(string trueLabel, string falseLabel, bool initialValue, Action<bool> onUpdate)
        {
            var checkBoxControl = new CheckBox 
            { 
                Content = initialValue ? trueLabel : falseLabel, 
                IsChecked = initialValue,
                FontSize = font_size,
                Margin = new Thickness(3)
            };
            checkBoxControl.FontSize = font_size;

            checkBoxControl.Checked += (sender, e) =>
            {
                checkBoxControl.Content = trueLabel;
                onUpdate(true);
            };
            checkBoxControl.Unchecked += (sender, e) =>
            {
                checkBoxControl.Content = falseLabel;
                onUpdate(false);
            };
            currentPanelToWriteTo.Children.Add(checkBoxControl);
        }


        private void AddComboBoxInput(string[] items, int initialValue, Action<int> onItemSelected)
        {
            var comboBoxControl = new ComboBox
            {
                FontSize = font_size,
                Margin = new Thickness(3)
            };
            comboBoxControl.Items = items;

            comboBoxControl.SelectionChanged += (sender, args) =>
            {
                if (comboBoxControl.SelectedIndex != -1)
                {
                    onItemSelected(comboBoxControl.SelectedIndex);
                }
            };
            comboBoxControl.SelectedIndex = initialValue;
            currentPanelToWriteTo.Children.Add(comboBoxControl);
        }

        private void AddFloatInput(string label, float initialValue, float minValue, float maxValue, Action<float> onUpdate)
        {
            var labelControl = new TextBlock 
            { 
                Text = $"{label}: {initialValue:0.00}",
                FontSize = font_size,
                Margin = new Thickness(3)
            };
            currentPanelToWriteTo.Children.Add(labelControl);

            var progressBarControl = new ProgressBar
            {
                Minimum = minValue,
                Maximum = maxValue,
                Value = initialValue,
                Height = 5,
                Margin = new Thickness(3)
            };
            progressBarControl.PointerPressed += (sender, e) =>
            {
                var progressBar = sender as ProgressBar;
                if (progressBar != null)
                {
                    var point = e.GetCurrentPoint(progressBar);
                    var ratio = point.Position.X / progressBar.Bounds.Width;
                    float newValue = (float)(minValue + ratio * (maxValue - minValue));
                    progressBar.Value = newValue;
                    labelControl.Text = $"{label}: {newValue:0.00}";
                    onUpdate(newValue);
                }
            };
            currentPanelToWriteTo.Children.Add(progressBarControl);
        }


        private void AddFloatTextInput(string label, float initialValue, Action<float> onTextChanged)
        {
            // Add the label control above the float text input control
            var labelControl = new TextBlock
            {
                Text = $"{label}: {initialValue:0.00}",
                FontSize = font_size,
                Margin = new Thickness(3)
            };
            currentPanelToWriteTo.Children.Add(labelControl);

            var floatTextInputControl = new TextBox
            {
                Text = initialValue.ToString("0.00"),
                Watermark = "Enter a float value"
            };

            floatTextInputControl.PropertyChanged += (sender, args) =>
            {
                if (args.Property == TextBox.TextProperty)
                {
                    if (float.TryParse(floatTextInputControl.Text, out float floatValue))
                    {
                        labelControl.Text = $"{label}: {floatValue:0.00}";
                        onTextChanged(floatValue);
                    }
                }
            };

            currentPanelToWriteTo.Children.Add(floatTextInputControl);
        }


        private DateTime lastUpdateTime = DateTime.MinValue;

        public static int Hash(params object[] args)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                foreach (var arg in args)
                {
                    hash = hash * 23 + (arg != null ? arg.GetHashCode() : 0);
                }
                return hash;
            }
        }

        private void TryUpdatingParameter(string playlistName, int playlistItem, Parameters parameter, float value)
        {
            int hash = Hash(playlistName, playlistItem, parameter, value);

            if (!addingNewItem && MainWindow.instance.bridgeConnection != null)
            {
                bool update = !_current_values.ContainsKey(hash);

                if (!update)
                {
                    update |= _current_values[hash] != hash;
                }

                if (update)
                {
                    MainWindow.instance.bridgeConnection.TryUpdatingParameter(playlistName, playlistItem, parameter, value);
                    _current_values[hash] = hash;
                }
            }
        }

        //private void TryUpdatingParameter(string playlistName, int playlistItem, Parameters parameter, float value)
        //{
        //    MainWindow.instance.bridgeConnection.TryUpdatingParameter(playlistName, playlistItem, parameter, value);
        //}


        // This is a hack and it will work but is easy to break
        // TODO fix this when I am not in a hurry
        StackPanel currentPanelToWriteTo;

        volatile bool addingNewItem = false;
        public void OnItemUpdated(Playlist playlist, PlaylistItem item)
        {
            addingNewItem = true;
            // Clear existing UI elements
            PropertiesStackPanel.Children.Clear();
            PropertiesBelowCollapseStackPanel.Children.Clear();

            // this is related to the hack above
            currentPanelToWriteTo = PropertiesStackPanel;

            // Add IntegerInputs for rows, cols, viewCount
            AddIntegerInput("Rows", item.rows, (newValue) =>
            {
                item.rows = newValue;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.rows, item.rows);
                playlist.UpdateItem(item);
            });

            AddIntegerInput("Cols", item.cols, (newValue) =>
            {
                item.cols = newValue;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.cols, item.cols);
                playlist.UpdateItem(item);
            });

            AddIntegerInput("View Count", item.viewCount, (newValue) =>
            {
                item.viewCount = newValue;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.viewCount, item.viewCount);
                playlist.UpdateItem(item);
            });

            AddFloatTextInput("Aspect Ratio", item.aspect, (newValue) =>
            {
                item.aspect = newValue;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.aspect, item.aspect);
                playlist.UpdateItem(item);
            });

            AddFloatInput("Crop Position X", item.crop_pos_x, -1, 1, (newValue) =>
            {
                item.crop_pos_x = newValue;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.crop_pos_x, item.crop_pos_x);
                playlist.UpdateItem(item);
            });

            AddFloatInput("Crop Position Y", item.crop_pos_y, -1, 1, (newValue) =>
            {
                item.crop_pos_y = newValue;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.crop_pos_y, item.crop_pos_y);
                playlist.UpdateItem(item);
            });

            AddFloatInput("Zoom", item.zoom, 0, 2, (newValue) =>
            {
                item.zoom = newValue;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.zoom, item.zoom);
                playlist.UpdateItem(item);
            });

            AddFloatInput("Duration (Sec)", item.durationMS / 1000, 0, 300, (newValue) =>
            {
                item.durationMS = (int)(newValue * 1000);
                //TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.durationMS, item.durationMS);
                playlist.UpdateItem(item);
            });

            // this is related to the hack above
            AddBoolInput("RGBD On", "RGBD Off", item.isRGBD == 1, (newValue) =>
            {
                item.isRGBD = newValue ? 1 : 0;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.isRGBD, item.isRGBD);
                PropertiesBelowCollapseStackPanel.IsVisible = newValue;
                playlist.UpdateItem(item);
            });

            currentPanelToWriteTo = PropertiesBelowCollapseStackPanel;
            PropertiesBelowCollapseStackPanel.IsVisible = item.isRGBD == 1;

            // Add ComboBoxInput for depth_loc
            AddComboBoxInput(new[] { "Bottom", "Top", "Left", "Right" }, item.depth_loc, (selectedIndex) =>
            {
                item.depth_loc = selectedIndex;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.depth_loc, item.depth_loc);
                playlist.UpdateItem(item);
            });

            // Add BoolInput for depth_inversion
            AddBoolInput("Depth Inversion On", "Depth Inversion Off", item.depth_inversion == 1, (newValue) =>
            {
                item.depth_inversion = newValue ? 1 : 0;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.depth_inversion, item.depth_inversion);
                playlist.UpdateItem(item);
            });

            // Add BoolInput for chroma_depth
            AddBoolInput("Chroma Depth On", "Chroma Depth Off", item.chroma_depth == 1, (newValue) =>
            {
                item.chroma_depth = newValue ? 1 : 0;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.chroma_depth, item.chroma_depth);
                playlist.UpdateItem(item);
            });

            AddFloatInput("Depthiness", item.depthiness, 0, 3, (newValue) =>
            {
                item.depthiness = newValue;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.depthiness, item.depthiness);
                playlist.UpdateItem(item);
            });

            AddFloatInput("Depth Cutoff", item.depth_cutoff, 0, 1, (newValue) =>
            {
                item.depth_cutoff = newValue;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.depth_cutoff, item.depth_cutoff);
                playlist.UpdateItem(item);
            });

            AddFloatInput("Focus", item.focus, -0.1f, 0.1f, (newValue) =>
            {
                item.focus = newValue;
                TryUpdatingParameter(playlist.name, playlist.items.IndexOf(item), Parameters.focus, item.focus);
                playlist.UpdateItem(item);
            });

            addingNewItem = false;
        }

    }
}