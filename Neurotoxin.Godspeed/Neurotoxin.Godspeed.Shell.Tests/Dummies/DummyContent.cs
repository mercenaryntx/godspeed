using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Tests.Helpers;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.Tests.Dummies
{
    public class DummyContent : FileSystemContentBase
    {
        private Tree<FileSystemItem> _tree;

        private FakingRules _fakingRules;
        public FakingRules FakingRules
        {
            get { return _fakingRules; }
            set
            {
                _fakingRules = value;
                if (_fakingRules.ItemTypes == null) _fakingRules.ItemTypes = new[] { ItemType.Directory, ItemType.File };
                if (_fakingRules.ItemTypesOnLevel == null)
                    _fakingRules.ItemTypesOnLevel = new Dictionary<int, ItemType[]>
                                                        {
                                                            {0, new[] {ItemType.Drive}}
                                                        };
                _tree = new Tree<FileSystemItem>(string.Empty, new FileSystemItem { Name = string.Empty, Path = string.Empty });
                var sw = new Stopwatch();
                sw.Start();
                GenerateTree(_tree, C.Random<int>(_fakingRules.TreeDepth));
                Console.WriteLine("[DDG] Generation completed in " + sw.Elapsed);
            }
        }

        public DummyContent() : base('/')
        {
        }

        private void GenerateTree(TreeItem<FileSystemItem> node, int depth, int level = 0)
        {
            var itemCount = C.Random<int>(_fakingRules.GetItemCount(level));
            var nodes = node.AddRange(C.CollectionOfFake<FileSystemItem>(itemCount, new { Type = _fakingRules.GetItemTypes(level), Name = new Range(8,13) }));
            foreach (var n in nodes)
            {
                n.Content.FullPath = n.Content.Path = node.Content.Path + "/" + n.Content.Name;
                n.Content.Size = C.Random<int>(0xFF, 0xFFFF);
                if (level < depth) GenerateTree(n, depth, level+1);
            }
        }

        public override IList<FileSystemItem> GetDrives()
        {
            return _tree.GetChildren("/");
        }

        public override IList<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new ArgumentNullException("path");
            return _tree.GetChildren(path);
        }

        public override FileSystemItem GetItemInfo(string itemPath, ItemType? type, bool swallowException)
        {
            var fake = C.Fake<FileSystemItem>();
            fake.Path = itemPath;
            if (type != null) fake.Type = type.Value;
            return fake;
        }

        public override bool DriveIsReady(string drive)
        {
            return true;
        }

        public override FileExistenceInfo FileExists(string path)
        {
            var fake = _tree.Find(path);
            if (fake != null && fake.Type == ItemType.File) return fake.Size;
            return false;
        }

        public override bool FolderExists(string path)
        {
            var fake = _tree.Find(path);
            return fake != null && fake.Type == ItemType.Directory;
        }

        public override void DeleteFolder(string path)
        {
            //TODO
        }

        public override void DeleteFile(string path)
        {
            //TODO
        }

        public override void CreateFolder(string path)
        {
            var parts = path.Split(Slash);
            var fake = C.Fake<FileSystemItem>();
            fake.Path = fake.FullPath = path;
            fake.Type = ItemType.Directory;
            fake.Name = parts.Last();
            _tree.Insert(path, fake);
        }

        public override FileSystemItem Rename(string path, string newName)
        {
            var fake = _tree.Find(path);
            fake.Name = newName;
            return fake;
        }

        public override Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition)
        {
            var fake = _tree.Find(path);
            if (fake == null)
            {
                if (access == FileAccess.Write)
                {
                    var parts = path.Split(new[] {Slash}, StringSplitOptions.RemoveEmptyEntries);
                    fake = C.Fake<FileSystemItem>();
                    fake.Path = fake.FullPath = path;
                    fake.Type = ItemType.File;
                    fake.Name = parts.Last();
                    _tree.Insert(path, fake);
                }
                else
                {
                    throw new Exception();
                }
            }

            var ms = new TreeItemStream(fake);
            ms.Seek(startPosition, SeekOrigin.Begin);
            return ms;
        }

        public override long? GetFreeSpace(string drive)
        {
            //TODO
            return 0;
        }

        public void AddFile(FileSystemItem item)
        {
            _tree.Insert(item.Path, item);
        }

    }
}