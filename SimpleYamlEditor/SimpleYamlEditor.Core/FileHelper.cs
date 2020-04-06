using System.IO;
using System.Linq;

namespace SimpleYamlEditor.Core
{
    public static class FileHelper
    {
        public static string[] GetFiles(string directoryName, bool recursive = false)
        {
            if (!recursive)
            {
                return Directory.GetFiles(directoryName);
            }

            var files = Directory.GetFiles(directoryName).ToList();
            foreach (var directory in Directory.GetDirectories(directoryName))
            {
                files.AddRange(GetFiles(directory));
            }

            return files.ToArray();
        }
    }
}