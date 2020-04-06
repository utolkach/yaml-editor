using System;
using System.IO;
using System.Linq;
using SimpleYamlEditor.Core;

namespace SimpleYamlEditor.Structurizer.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = args.FirstOrDefault();
            if (!Directory.Exists(path))
            {
                Console.Write("Not existing directory!");
                return;
            }

            var files = FileHelper.GetFiles(path, recursive: true);

            foreach (var file in files)
            {
                using var sr = new StreamReader(file);
                var struturedFile = YamlHelper.StructureYamlFile(sr.ReadToEnd());
                using var sw = new StreamWriter(file, false);
                sw.Write(struturedFile);
            }
        }
    }
}
