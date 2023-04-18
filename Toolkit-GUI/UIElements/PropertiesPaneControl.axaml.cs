using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using ToolkitGUI.Media;

namespace ToolkitGUI
{
    public partial class PropertiesPaneControl : UserControl
    {
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


        private void AddComboBoxInput(string[] items, Action<int> onItemSelected)
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

        // This is a hack and it will work but is easy to break
        // TODO fix this when I am not in a hurry
        StackPanel currentPanelToWriteTo;

        public void OnItemUpdated(Playlist playlist, PlaylistItem item)
        {
            // Clear existing UI elements
            PropertiesStackPanel.Children.Clear();
            PropertiesBelowCollapseStackPanel.Children.Clear();

            // this is related to the hack above
            currentPanelToWriteTo = PropertiesStackPanel;

            // Add IntegerInputs for rows, cols, viewCount
            AddIntegerInput("Rows", item.rows, (newValue) =>
            {
                item.rows = newValue;
                playlist.UpdateItem(item);
            });

            AddIntegerInput("Cols", item.cols, (newValue) =>
            {
                item.cols = newValue;
                playlist.UpdateItem(item);
            });

            AddIntegerInput("View Count", item.viewCount, (newValue) =>
            {
                item.viewCount = newValue;
                playlist.UpdateItem(item);
            });

            // Add FloatTextInput for aspect
            AddFloatTextInput("Aspect Ratio", item.aspect, (newValue) =>
            {
                item.aspect = newValue;
                playlist.UpdateItem(item);
            });

            // this is related to the hack above
            AddBoolInput("RGBD On", "RGBD Off", item.isRGBD == 1, (newValue) =>
            {
                item.isRGBD = newValue ? 1 : 0;
                PropertiesBelowCollapseStackPanel.IsVisible = newValue;
                playlist.UpdateItem(item);
            });
            currentPanelToWriteTo = PropertiesBelowCollapseStackPanel;
            PropertiesBelowCollapseStackPanel.IsVisible = item.isRGBD == 1;

            // Add ComboBoxInput for depth_loc
            AddComboBoxInput(new[] { "Bottom", "Top", "Left", "Right" }, (selectedIndex) =>
            {
                item.depth_loc = selectedIndex;
                playlist.UpdateItem(item);
            });

            // Add BoolInput for depth_inversion
            AddBoolInput("Depth Inversion On", "Depth Inversion Off", item.depth_inversion == 1, (newValue) =>
            {
                item.depth_inversion = newValue ? 1 : 0;
                playlist.UpdateItem(item);
            });

            // Add BoolInput for chroma_depth
            AddBoolInput("Chroma Depth On", "Chroma Depth Off", item.chroma_depth == 1, (newValue) =>
            {
                item.chroma_depth = newValue ? 1 : 0;
                playlist.UpdateItem(item);
            });

            // Add FloatInputs for crop_pos_x, crop_pos_y, depthiness, depth_cutoff, focus, and zoom
            AddFloatInput("Crop Position X", item.crop_pos_x, -1, 1, (newValue) =>
            {
                item.crop_pos_x = newValue;
                playlist.UpdateItem(item);
            });

            AddFloatInput("Crop Position Y", item.crop_pos_y, -1, 1, (newValue) =>
            {
                item.crop_pos_y = newValue;
                playlist.UpdateItem(item);
            });

            AddFloatInput("Depthiness", item.depthiness, 0, 1, (newValue) =>
            {
                item.depthiness = newValue;
                playlist.UpdateItem(item);
            });

            AddFloatInput("Depth Cutoff", item.depth_cutoff, 0, 1, (newValue) =>
            {
                item.depth_cutoff = newValue;
                playlist.UpdateItem(item);
            });

            AddFloatInput("Focus", item.focus, -1, 1, (newValue) =>
            {
                item.focus = newValue;
                playlist.UpdateItem(item);
            });

            AddFloatInput("Zoom", item.zoom, 0, 2, (newValue) =>
            {
                item.zoom = newValue;
                playlist.UpdateItem(item);
            });
        }

    }
}