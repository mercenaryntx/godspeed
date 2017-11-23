using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Controllers
{
    public class ResourceManager : IResourceManager
    {
        private readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();

        public byte[] GetContentByteArray(string path)
        {
            if (_cache.ContainsKey(path)) return _cache[path];

            var contentInfo = Application.GetResourceStream(new Uri(path, UriKind.Relative));
            if (contentInfo == null) throw new ArgumentException(string.Format("Content not found: {0}", path));
            var ms = new MemoryStream();
            contentInfo.Stream.CopyTo(ms);
            var bytes = ms.ToArray();
            _cache.Add(path, bytes);
            return bytes;
        }
    }
}