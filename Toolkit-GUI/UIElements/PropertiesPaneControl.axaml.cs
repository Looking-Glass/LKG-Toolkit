using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolkit_API.Bridge.Params;
using ToolkitGUI.Media;

namespace ToolkitGUI
{
    public partial class PropertiesPaneControl : UserControl
    {
        Dictionary<int, int> _current_values = new Dictionary<int, int>();

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
            var labelControl = new TextBlock { Text = label };
            currentPanelToWriteTo.Children.Add(labelControl);

            var inputControl = new NumericUpDown { Value = initialValue };
            inputControl.ValueChanged += (sender, e) => onUpdate((int)e.NewValue);
            currentPanelToWriteTo.Children.Add(inputControl);
        }

        private void AddBoolInput(string trueLabel, string falseLabel, bool initialValue, Action<bool> onUpdate)
        {
            var checkBoxControl = new CheckBox { Content = initialValue ? trueLabel : falseLabel, IsChecked = initialValue };
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
            var comboBoxControl = new ComboBox();
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
            var labelControl = new TextBlock { Text = $"{label}: {initialValue:0.00}" };
            currentPanelToWriteTo.Children.Add(labelControl);

            var sliderControl = new Slider { Minimum = minValue, Maximum = maxValue, Value = initialValue };
            sliderControl.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Slider.ValueProperty)
                {
                    float newValue = (float)sliderControl.Value;
                    labelControl.Text = $"{label}: {newValue:0.00}";
                    onUpdate(newValue);
                }
            };
            currentPanelToWriteTo.Children.Add(sliderControl);
        }

        private void AddFloatTextInput(string label, float initialValue, Action<float> onTextChanged)
        {
            // Add the label control above the float text input control
            var labelControl = new TextBlock { Text = $"{label}: {initialValue:0.00}" };
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

        //private void TryUpdatingParameter(string playlistName, int playlistItem, Parameters parameter, float value)
        //{
        //    int hash = Hash(playlistName, playlistItem, parameter, value);

        //    if (!addingNewItem && MainWindow.instance.bridgeConnection != null)
        //    {
        //        bool update = !_current_values.ContainsKey(hash);

        //        if (!update)
        //        {
        //            update |= _current_values[hash] != hash;
        //        }

        //        if (update)
        //        {
        //            MainWindow.instance.bridgeConnection.TryUpdatingParameter(playlistName, playlistItem, parameter, value);
        //            _current_values[hash] = hash;
        //        }
        //    }
        //}

        private System.Timers.Timer debounceTimer;
        private (string playlistName, int playlistItem, Parameters parameter, float value) latestUpdate;

        private void TryUpdatingParameter(string playlistName, int playlistItem, Parameters parameter, float value)
        {
            if (debounceTimer == null)
            {
                debounceTimer = new System.Timers.Timer(250); // 0.1 second delay
                debounceTimer.Elapsed += (sender, e) => SendLatestUpdate();
                debounceTimer.AutoReset = false; // Only trigger once
            }

            // Save the latest values
            latestUpdate = (playlistName, playlistItem, parameter, value);

            // Restart the timer
            debounceTimer.Stop();
            debounceTimer.Start();
        }

        private void SendLatestUpdate()
        {
            if (!addingNewItem && MainWindow.instance.bridgeConnection != null)
            {
                MainWindow.instance.bridgeConnection.TryUpdatingParameter(latestUpdate.playlistName, latestUpdate.playlistItem, latestUpdate.parameter, latestUpdate.value);
            }
        }


        //private void TryUpdatingParameter(string playlistName, int playlistItem, Parameters parameter, float value)
        //{
        //    // I want to essentially debounce this, and only send one every 0.1 seconds, but I want to save the parameter and the value and send the most up to date value when it trys to send, this is called by sliders and seems to be breaking
        //    if(!addingNewItem && MainWindow.instance.bridgeConnection != null)
        //    {
        //        MainWindow.instance.bridgeConnection.TryUpdatingParameter(playlistName, playlistItem, parameter, value);
        //    }
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

            AddFloatInput("Depthiness", item.depthiness, 0, 1, (newValue) =>
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