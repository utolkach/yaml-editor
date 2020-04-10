using System.Collections.ObjectModel;

namespace SimpleYamlEditor.AvaloniaGui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<KeyValue> List { get; set; } = new ObservableCollection<KeyValue>();
    }
}
