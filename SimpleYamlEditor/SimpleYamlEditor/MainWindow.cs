using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SimpleYamlEditor
{
    public partial class MainWindow : Form
    {

        public static string DirectoryPath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            // enabling Double buffering as said here: https://10tec.com/articles/why-datagridview-slow.aspx
            if (!SystemInformation.TerminalServerSession)
            {
                var dgvType = grid.GetType();
                var pi = dgvType.GetProperty("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(grid, true, null);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            grid.Rows.Clear();
            grid.Columns.Clear();

            var result = openFileDialog.ShowDialog(this);
            if (result != DialogResult.OK)
            {
                return;
            }

            var path = openFileDialog.FileName;
            var directoryName = Path.GetDirectoryName(path);
            DirectoryPath = directoryName;
            var configs = LoadYaml(directoryName).OrderBy(x => x.Item1).ToList();
            var keys = configs.SelectMany(x => x.Item2.Keys).Distinct().OrderBy(x => x).ToList();
            grid.Columns.Add("ConfigName", "Config name");
            configs.ForEach(x => grid.Columns.Add(x.Item1, x.Item1));
            keys.ForEach(key =>
            {
                var values = configs
                    .Select(env => env.Item2.TryGetValue(key, out var v) ? v as string : string.Empty)
                    .ToArray();
                var list = (new[] {key}).ToList();
                list.AddRange(values);
                grid.Rows.Add(list.ToArray());
            });

            grid.Columns[0].Frozen = true;
            grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
            grid.AutoResizeColumn(0, DataGridViewAutoSizeColumnMode.AllCells);

            MarkEmptyCells();

            btnSave.Text = "Save configs";
        }

        private void MarkEmptyCells()
        {
            for (var c = 1; c < grid.ColumnCount; c++)
            {
                for (var r = 1; r < grid.RowCount; r++)
                {
                    if (grid[c, r].Value == string.Empty)
                    {
                        grid[c, r].Style = new DataGridViewCellStyle { BackColor = Color.LavenderBlush };
                    }
                }
            }

        }

        private IEnumerable<(string, Dictionary<string, object>)> LoadYaml(string directoryName)
        {
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            foreach (var file in Directory.GetFiles(directoryName))
            {
                using (var sr = new StreamReader(file))
                {
                    object obj = null;
                    try
                    {
                        obj = deserializer.Deserialize(new StringReader(sr.ReadToEnd()));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Can't parse:{file}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    yield return (Path.GetFileName(file),
                        JsonHelper.DeserializeAndFlatten(JsonConvert.SerializeObject(obj)));
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            for (var c = 1; c < grid.ColumnCount; c++)
            {
                var filename = grid.Columns[c].HeaderText;
                using (var sw = new StreamWriter(Path.Combine(DirectoryPath, filename), false))
                {
                    for (var r = 1; r < grid.RowCount; r++)
                    {
                        var value = grid[c, r].Value as string;
                        var key = grid[0, r].Value as string;
                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                            sw.WriteLine($"{key}: '{value}'");
                    }
                }

            }

            btnSave.Text = "Saved!";
        }
    }
}
