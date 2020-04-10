using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;
using SimpleYamlEditor.AvaloniaGui.ViewModels;
using SimpleYamlEditor.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;

namespace SimpleYamlEditor.AvaloniaGui.Views
{
    public class MainWindow : Window
    {
        private MainWindowViewModel _vm;
        private string[] _envs;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.DataContext = new MainWindowViewModel();
        }

        public async void exit(object sender, CancelEventArgs e)
        {
        }

        public async void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            var directoryName = await GetPathToOpenAsync();
            if (directoryName == null) return;
            List<(string, Dictionary<string, object>)> configs = null;
            try
            {
                configs = YamlHelper.LoadYamlAsDictionaries(directoryName).OrderBy(x => x.Item1).ToList();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }

            var keys = configs.SelectMany(x => x.Item2.Keys).Distinct().OrderBy(x => x).ToList();
            var configsConverted = ConvertToViewModel(configs, keys).ToList();
            FillGrid(configsConverted);
        }

        private void FillGrid(IList<KeyValue> configsConverted)
        {
            _vm = new MainWindowViewModel
            {
                List = new ObservableCollection<KeyValue>(configsConverted)
            };
            this.DataContext = _vm;
            _envs = configsConverted.SelectMany(x => x.EnvValues.Select(y => y.Env)).Distinct().ToArray();
            
            var envsCount = _envs.Count();
            var columns = Enumerable.Range(0, envsCount - 1).Select(env => new DataGridTextColumn()
            {
                Binding = new Binding($"EnvValues[{env}].Value", BindingMode.TwoWay),
                Header = _envs[env],
                CanUserResize = true,
                CanUserReorder = true,
                CanUserSort = true,
                Width = new DataGridLength(100, DataGridLengthUnitType.SizeToHeader),
            });
            var dataGrid = this.FindControl<DataGrid>("grid");
            dataGrid.Columns.AddRange(columns);

        }

        private static IEnumerable<KeyValue> ConvertToViewModel(List<(string, Dictionary<string, object>)> configs, List<string> keys)
        {
            return keys.Select(k => new KeyValue()
            {
                Key = k,
                EnvValues = configs.Select(c =>
                {
                    var v1 = c.Item2.TryGetValue(k, out var v) ? v : String.Empty;
                    return (c.Item1, v1);
                }).Select(x => new KeyValue.EnvValue() { Env = x.Item1, Value = x.Item2.ToString() }).ToList()
            });
        }

        public async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            
            //yaml: key-values
            foreach (var env in _envs)
            {
                using var sw = new StreamWriter(Path.Combine(DirectoryPath, env), false);
                foreach (var row in _vm.List)
                {
                    var value = row.EnvValues.First(x => x.Env == env).Value;
                    var key = row.Key;
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {
                        sw.WriteLine($"{key}: '{value}'");
                    }
                }
                sw.Dispose();
            }

            if (this.FindControl<CheckBox>("chkStructured").IsChecked.Value)
            {
                foreach (var env in _envs)
                {
                    using var sr = new StreamReader(Path.Combine(DirectoryPath, env));
                    var struturedFile = YamlHelper.StructureYamlFile(sr.ReadToEnd());
                    sr.Dispose();
                    using var sw = new StreamWriter(Path.Combine(DirectoryPath, env), false);
                    sw.Write(struturedFile);
                    sw.Dispose();
                }
            }
        }

        private static void ShowErrorMessage(string text)
        {
            var model = new ErrorMessageViewModel()
            {
                ErrorMessageText = text
            };
            var messagBox = new ErrorMessage()
            {
                DataContext = model
            };
            messagBox.Show();
        }

        private async Task<string> GetPathToOpenAsync()
        {
            var openFileDialog = new OpenFileDialog();
            var result = await openFileDialog.ShowAsync(this);

            var directoryName = Path.GetDirectoryName(result.FirstOrDefault());
            DirectoryPath = directoryName;
            return directoryName;
        }

        public string DirectoryPath { get; set; }
    }
}
