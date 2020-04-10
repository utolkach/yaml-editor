using System.Collections.Generic;

namespace SimpleYamlEditor.AvaloniaGui.ViewModels
{
    public class KeyValue
    {
        public string Key { get; set; }
        public List<EnvValue> EnvValues { get; set; }

        public class EnvValue
        {
            public string Env { get; set; }
            public string Value { get; set; }
        }
    }
}