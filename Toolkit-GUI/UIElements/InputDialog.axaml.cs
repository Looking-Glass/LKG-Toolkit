using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace LookingGlass.Toolkit.GUI
{
    public partial class InputDialog : Window
    {
        public InputDialog()
        {
            AvaloniaXamlLoader.Load(this);
            PromptTextBlock = this.FindControl<TextBlock>("PromptTextBlock");
            InputTextBox = this.FindControl<TextBox>("InputTextBox");
        }

        public InputDialog(string title, string prompt) : this()
        {
            AvaloniaXamlLoader.Load(this);
            PromptTextBlock = this.FindControl<TextBlock>("PromptTextBlock");
            InputTextBox = this.FindControl<TextBox>("InputTextBox");

            this.Title = title;
            PromptTextBlock.Text = prompt;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close(InputTextBox.Text);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
