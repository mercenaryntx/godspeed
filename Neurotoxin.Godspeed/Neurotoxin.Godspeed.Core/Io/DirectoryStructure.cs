using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Neurotoxin.Godspeed.Core.Extensions;

namespace Neurotoxin.Godspeed.Core.Io
{
    public static class DirectoryStructure
    {
        private static XmlDocument _xml;

        public static IEnumerable<string> WellKnownDirectoriesOf(string path)
        {
            var result = new List<string>();

            if (_xml == null)
            {
                var assembly = Assembly.GetAssembly(typeof(DirectoryStructure));
                var rStream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".g.resources");
                var resourceReader = new System.Resources.ResourceReader(rStream);
                var items = resourceReader.OfType<System.Collections.DictionaryEntry>();
                using (var stream = (UnmanagedMemoryStream)items.First(x => x.Key.Equals("resources/hdddirectorystructure.xml")).Value)
                {
                    _xml = new XmlDocument();
                    _xml.Load(stream);
                }
            }

            var splitter = new Regex(@"[/\\]");
            var currentDir = _xml.SelectSingleNode("/Drive");
            var parts = new Queue<string>(splitter.Split(path));
            var foundEntryPoint = false;
            while (currentDir != null && parts.Count > 0)
            {
                var p = parts.Dequeue();
                if (string.IsNullOrEmpty(p)) continue;
                var subDir = currentDir.ChildNodes.Cast<XmlElement>().FirstOrDefault(child =>
                    {
                        if (!child.HasAttribute("Name")) return false;
                        var r = new Regex(string.Format("^{0}$", child.GetAttribute("Name")));
                        return r.IsMatch(p);
                    });
                if (foundEntryPoint)
                {
                    currentDir = subDir;
                }
                else if (subDir != null)
                {
                    foundEntryPoint = true;
                    currentDir = subDir;
                }
            }

            if (currentDir != null)
            {
                foreach (XmlElement childNode in currentDir.ChildNodes)
                {
                    if (childNode.HasAttribute("Name"))
                    {
                        result.Add(childNode.GetAttribute("Name"));
                    }
                    else if (childNode.HasAttribute("Value") && childNode.HasAttribute("Type"))
                    {
                        var type = Type.GetType(childNode.GetAttribute("Type"));
                        if (type.IsEnum)
                        {
                            var enumValue = Enum.Parse(type, childNode.GetAttribute("Value"), true);
                            var a = BitConverter.GetBytes((int)enumValue);
                            Array.Reverse(a);
                            result.Add(a.ToHex());
                        }
                    }
                }
            }

            return result;
        }
    }
}