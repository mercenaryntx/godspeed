using System;
using System.Collections.Generic;
using System.IO;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Tests.Helpers;

namespace Neurotoxin.Godspeed.Shell.Tests.Dummies
{
    public class TreeItemStream : MemoryStream
    {
        private readonly FileSystemItem _item;

        public TreeItemStream(FileSystemItem item) : base(0xFFFF)
        {
            _item = item;
            GenerateContent();
        }

        private void GenerateContent()
        {
            if (!_item.Size.HasValue) return;
            var size = (int)_item.Size.Value;
            var bytes = C.Random<byte[]>(size, size);
            Write(bytes, 0, size);
            Flush();
        }

        protected override void Dispose(bool disposing)
        {
            _item.Size = Length;
            base.Dispose(disposing);
        }
    }
}