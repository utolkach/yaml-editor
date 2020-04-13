using SimpleYamlEditor.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

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

            string directoryName;
            bool ret;
            var result = openFileDialog.ShowDialog(this);
            if (result != DialogResult.OK)
            {
                directoryName = string.Empty;
                ret = true;
            }
            else
            {
                var path = openFileDialog.FileNames.First();
                DirectoryPath = Path.GetDirectoryName(path);
                ret = false;
            }

            if (ret) return;
            List<(string, Dictionary<string, object>)> configs = null;
            try
            {
                configs = YamlHelper.LoadYamlAsDictionaries(openFileDialog.FileNames.ToList()).OrderBy(x => x.Item1).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Can't parse: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            var keys = configs.SelectMany(x => x.Item2.Keys).Distinct().OrderBy(x => x).ToList();
            FillGrid(configs, keys);

            btnSave.Text = "Save configs";
        }

        private void FillGrid(List<(string, Dictionary<string, object>)> configs, List<string> keys)
        {
            grid.Columns.Add("ConfigName", "Config name");
            configs.ForEach(x => grid.Columns.Add(x.Item1, x.Item1));
            keys.ForEach(key =>
            {
                var values = configs
                    .Select(env => env.Item2.TryGetValue(key, out var v) ? v as string : string.Empty)
                    .ToArray();
                var list = (new[] { key }).ToList();
                list.AddRange(values);
                grid.Rows.Add(list.ToArray());
            });

            grid.Columns[0].Frozen = true;
            grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
            grid.AutoResizeColumn(0, DataGridViewAutoSizeColumnMode.AllCells);

            MarkEmptyCells();
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            for (var c = 1; c < grid.ColumnCount; c++)
            {
                var filename = grid.Columns[c].HeaderText;
                using (var sw = new StreamWriter(Path.Combine(DirectoryPath, filename), false))
                {
                    for (var r = 0; r < grid.RowCount; r++)
                    {
                        var value = grid[c, r].Value as string;
                        var key = grid[0, r].Value as string;
                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        {
                            sw.WriteLine($"{key}: '{value}'");
                        }
                    }

                }
            }

            btnSave.Text = "Saved!";

            if (checkBox1.Checked)
            {
                for (var c = 1; c < grid.ColumnCount; c++)
                {
                    var filename = grid.Columns[c].HeaderText;

                    using var sr = new StreamReader(Path.Combine(DirectoryPath, filename));
                    var struturedFile = YamlHelper.StructureYamlFile(sr.ReadToEnd());
                    sr.Dispose();
                    using var sw = new StreamWriter(Path.Combine(DirectoryPath, filename), false);
                    sw.Write(struturedFile);
                    sw.Dispose();
                }
            }
        }
    }
}
