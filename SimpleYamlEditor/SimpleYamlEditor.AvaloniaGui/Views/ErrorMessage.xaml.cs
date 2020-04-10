using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SimpleYamlEditor.AvaloniaGui.Views
{
    public class ErrorMessage : Window
    {
        public ErrorMessage()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
