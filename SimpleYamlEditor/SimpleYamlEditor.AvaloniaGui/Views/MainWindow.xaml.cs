using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;
using SimpleYamlEditor.AvaloniaGui.ViewModels;
using SimpleYamlEditor.Core;

namespace SimpleYamlEditor.AvaloniaGui.Views
{
    public class MainWindow : Window
    {
        private MainWindowViewModel _vm;
        private string[] _envs;
        private bool _isDirty;
        private DataGrid Grid => this.FindControl<DataGrid>("grid");
        public string DirectoryPath { get; set; }

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public async void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
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
                    ShowPopup<ErrorMessage>(ex.Message);
                }

                var keys = configs.SelectMany(x => x.Item2.Keys).Distinct().OrderBy(x => x).ToList();
                var configsConverted = ViewModelHelper.ConvertToViewModel(configs, keys).ToList();
                FillGrid(configsConverted);
                this.Title = "SimpleYamlEditor";
            }
            catch (Exception exception)
            {
            }
        }

        public void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _vm.List.Add(new KeyValue
                {
                    Key = "New key",
                    EnvValues = _envs.Select(x => new EnvValue
                    {
                        Env = x,
                        Value = "New value"
                    }).ToList()
                });
                Grid.SelectedIndex = _vm.List.Count - 1;
                Grid.ScrollIntoView(_vm.List[Grid.SelectedIndex], null);
            }
            catch (Exception exception)
            {
            }
        }

        public void btnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _vm.List.RemoveAt(Grid.SelectedIndex);
            }
            catch (Exception exception)
            {
            }
        }

        public void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception exception)
            {
            }
        }

        public void Exit(object sender, CancelEventArgs e)
        {
            //no op yet
        }

        private void FillGrid(IList<KeyValue> configsConverted)
        {
            Grid.Columns.RemoveMany(Grid.Columns.Where(x => x != Grid.Columns.First()));
            _vm = new MainWindowViewModel
            {
                List = new ObservableCollection<KeyValue>(configsConverted)
            };

            DataContext = _vm;

            _envs = configsConverted.SelectMany(x => x.EnvValues.Select(y => y.Env)).Distinct().ToArray();

            var envsCount = _envs.Count();
            var columns = Enumerable.Range(0, envsCount).Select(env => new DataGridTextColumn
            {
                Binding = new Binding($"EnvValues[{env}].Value", BindingMode.TwoWay),
                Header = _envs[env],
                CanUserResize = true,
                CanUserReorder = true,
                CanUserSort = true,
                Width = new DataGridLength(100, DataGridLengthUnitType.SizeToHeader)
            });

            var dataGrid = Grid;
            dataGrid.Columns.AddRange(columns);
            dataGrid.CellEditEnded += (x, e) =>
            {

                _isDirty = true;
                this.Title = "SimpleYamlEditor [MODIFIED]";
            };

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = new MainWindowViewModel();
        }

        private static void ShowPopup<T>(string text) where T : Window, new()
        {
            var model = new ErrorMessageViewModel
            {
                ErrorMessageText = text
            };
            var messagBox = new T
            {
                DataContext = model
            };
            messagBox.Show();
        }

        private async Task<List<string>> GetPathToOpenAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                AllowMultiple = true
            };

            var result = await openFileDialog.ShowAsync(this);

            var directoryName = Path.GetDirectoryName(result.FirstOrDefault());
            DirectoryPath = directoryName;
            return result.ToList();
        }
    }
}
