using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SimpleYamlEditor.AvaloniaGui.Views
{
    public class Prompt : Window
    {
        public Prompt()
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

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close(true);
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }

    }
}
