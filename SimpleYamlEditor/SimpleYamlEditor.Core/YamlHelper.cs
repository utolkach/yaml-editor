using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SimpleYamlEditor.Core
{
    public static class YamlHelper
    {
        public static  IEnumerable<(string, Dictionary<string, object>)> LoadYamlAsDictionaries(string directoryName)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var files = FileHelper.GetFiles(directoryName);
            foreach (var file in files)
            {
                object obj = null;
                    string str;

                    using (var sr = new StreamReader(file))
                    {

                        str = sr.ReadToEnd();
                    }

                    var stringReader = new StringReader(str);
                    obj = deserializer.Deserialize(stringReader);


                yield return (Path.GetFileName(file), JsonHelper.DeserializeAndFlatten(JsonConvert.SerializeObject(obj)));
            }
        }

        public static string StructureYamlFile(string content)
        {
            var yamlStream = LoadStringIntoYamlStream(content);
            yamlStream = StructureYaml(yamlStream);
            return YamlStreamToString(yamlStream);
        }

        private static YamlStream StructureYaml(YamlStream yaml)
        {
            var root = yaml.Documents[0].RootNode as YamlMappingNode;

            var newroot = new YamlMappingNode();
            foreach (var child in root.Children)
            {
                var row = child.Key.ToString();

                if (row.Contains(":"))
                {
                    var temp = row;
                    var selectors = temp.Split(':').ToList();
                    var value = child.Value.ToString();
                    selectors.Add(value);

                    var configKey = selectors[selectors.Count - 2];
                    var configValue = new YamlScalarNode(selectors[selectors.Count - 1])
                    {
                        Style = ScalarStyle.SingleQuoted
                    };

                    var tailWithValue = new KeyValuePair<YamlNode, YamlNode>(configKey, configValue);

                    var targetChildren = newroot.Children;

                    var nestedSelectors = selectors.Count - 2;
                    for (var i = 0; i <= nestedSelectors; i++)
                    {
                        var currentKey = selectors[i];

                        if (targetChildren.Keys.Select(x => x.ToString()).Contains(currentKey))
                        {
                            //just copy value
                            if (targetChildren[currentKey] is YamlScalarNode)
                            {
                                newroot.Children.Add(row, value); break;
                            }

                            //add to existing key
                            if (targetChildren[currentKey] is YamlMappingNode)
                            {
                                if (i < nestedSelectors)
                                {
                                    targetChildren = (targetChildren[currentKey] as YamlMappingNode)?.Children;
                                }

                                if (i == nestedSelectors)
                                {
                                    targetChildren.Add(tailWithValue);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // add nested keys
                            if (i < nestedSelectors)
                            {
                                targetChildren.Add(new KeyValuePair<YamlNode, YamlNode>(currentKey, new YamlMappingNode()));
                                targetChildren = (targetChildren[currentKey] as YamlMappingNode)?.Children;
                            }
                            if (i == nestedSelectors)
                            {
                                targetChildren.Add(tailWithValue);
                            }
                        }
                    }
                }
                else
                {
                    newroot.Children.Add(child);
                }
            }

            return new YamlStream
            {
                new YamlDocument(newroot)
            };
        }

        private static YamlStream LoadStringIntoYamlStream(string content)
        {
            var yaml = new YamlStream();
            yaml.Load(new StringReader(content));
            return yaml;
        }

        private static string YamlStreamToString(YamlStream yamlStream)
        {
            using (var sw = new StringWriter())
            {
                yamlStream.Save(sw, false);
                return sw.ToString();
            }
        }

        private static void Sort(ref YamlNode node)
        {
            if (node is YamlMappingNode yamlMappingNode)
            {
                node = new YamlMappingNode(yamlMappingNode.Children.OrderBy(x => x.Key));
                foreach (var childrenKey in yamlMappingNode.Children.Keys)
                {
                    yamlMappingNode.Children.TryGetValue(childrenKey, out var newley);
                    Sort(ref newley);
                }
            }
        }
    }
}
