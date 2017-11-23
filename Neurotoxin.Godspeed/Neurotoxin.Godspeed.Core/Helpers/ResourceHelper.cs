using System.IO;
using System.Linq;
using System.Reflection;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Helpers
{
    public static class ResourceHelper
    {
        public static SvodPackage GetEmptyConHeader()
        {
            return ModelFactory.GetModel<SvodPackage>(GetResourceBytes("ConHeader.bin"));
        }

        public static byte[] GetResourceBytes(string resourceName)
        {
            var assembly = Assembly.GetAssembly(typeof(ResourceHelper));
            var rStream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".g.resources");
            var resourceReader = new System.Resources.ResourceReader(rStream);
            var items = resourceReader.OfType<System.Collections.DictionaryEntry>();
            var ums = (UnmanagedMemoryStream)items.First(x => x.Key.Equals("resources/" + resourceName.ToLower())).Value;
            var bytes = new byte[ums.Length];
            ums.Read(bytes, 0, bytes.Length);
            return bytes;
        }
    }
}