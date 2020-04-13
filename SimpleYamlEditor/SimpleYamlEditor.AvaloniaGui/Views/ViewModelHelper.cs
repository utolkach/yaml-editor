using System;
using System.Collections.Generic;
using System.Linq;
using SimpleYamlEditor.AvaloniaGui.ViewModels;

namespace SimpleYamlEditor.AvaloniaGui.Views
{
    public class ViewModelHelper
    {
        public static IEnumerable<KeyValue> ConvertToViewModel(List<(string, Dictionary<string, object>)> configs, List<string> keys)
        {
            return keys.Select(k => new KeyValue()
            {
                Key = k,
                EnvValues = configs.Select(c =>
                {
                    var v1 = c.Item2.TryGetValue(k, out var v) ? v : String.Empty;
                    return (c.Item1, v1);
                }).Select(x => new EnvValue() { Env = x.Item1, Value = x.Item2.ToString() }).ToList()
            });
        }
    }
}