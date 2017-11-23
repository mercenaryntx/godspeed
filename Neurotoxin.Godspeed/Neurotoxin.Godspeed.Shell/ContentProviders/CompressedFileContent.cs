using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;
using SharpCompress.Archive;
using SharpCompress.Archive.Rar;
using SharpCompress.Archive.SevenZip;
using SharpCompress.Common;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class CompressedFileContent : FileSystemContentBase
    {
        private string _archivePath;
        private IArchive _archive;
        private Tree<FileSystemItem> _fileStructure;
        private FileSystemItem _drive;

        private const char SLASH = '/';
        private const char BACKSLASH = '\\';

        public override IList<FileSystemItem> GetDrives()
        {
            return new List<FileSystemItem> { _drive };
        }

        public override IList<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new ArgumentException();
            return _fileStructure.GetChildren(path);
        }

        public override FileSystemItem GetItemInfo(string path, ItemType? type, bool swallowException)
        {
            if (type.HasValue && type.Value == ItemType.Directory && _archive is RarArchive && path[path.Length -1] != BACKSLASH) path += BACKSLASH;
            var item = _fileStructure.Find(path);
            if (item == null) return null;
            if (type != null)
            {
                if ((type == ItemType.File && item.Type != ItemType.File) ||
                    (type != ItemType.File && item.Type == ItemType.File)) return null;
                item.Type = type.Value;
            }
            return item;
        }

        private FileSystemItem CreateModel(IArchiveEntry entry)
        {
            DateTime date = _archive is SevenZipArchive
                                ? DateTime.MinValue
                                : (entry.LastModifiedTime ??
                                   entry.LastAccessedTime ??
                                   entry.CreatedTime ??
                                   entry.ArchivedTime ??
                                   DateTime.MinValue);

            return CreateModel(entry.FilePath, 
                               entry.IsDirectory,
                               date,
                               entry.IsDirectory ? (long?) null : entry.Size);
        }

        private FileSystemItem CreateModel(string path, bool isDirectory, DateTime date, long? size)
        {
            var slash = Slash;
            var name = path.TrimEnd(slash);
            var slashIndex = name.LastIndexOf(slash);
            if (slashIndex > -1) name = name.Substring(slashIndex + 1);

            if (isDirectory && path[path.Length - 1] != slash) path += slash;

            return new FileSystemItem
            {
                Name = name,
                Type = isDirectory ? ItemType.Directory : ItemType.File,
                Path = path,
                FullPath = string.Format(@"{0}:\{1}", _archivePath, path),
                Date = date,
                Size = size
            };
        }

        public override bool DriveIsReady(string drive)
        {
            return true;
        }

        public override FileExistenceInfo FileExists(string path)
        {
            var item = _fileStructure.Find(path);
            if (item == null || item.Type != ItemType.File) return false;
            return item.Size;
        }

        public override bool FolderExists(string path)
        {
            var item = _fileStructure.Find(path);
            return item != null && (item.Type == ItemType.Directory || item.Type == ItemType.Drive);
        }

        public override void DeleteFolder(string path)
        {
            throw new NotSupportedException();
        }

        public override void DeleteFile(string path)
        {
            throw new NotSupportedException();
        }

        public override void CreateFolder(string path)
        {
            throw new NotSupportedException();
        }

        public override FileSystemItem Rename(string path, string newName)
        {
            throw new NotImplementedException();
        }

        public override Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition)
        {
            if (access != FileAccess.Read) throw new ArgumentException("Write is not supported");
            var stream = _archive.Entries.First(e => e.FilePath == path).OpenEntryStream();
            if (startPosition != 0) stream.Seek(startPosition, SeekOrigin.Begin);
            return stream;
        }

        public override long? GetFreeSpace(string drive)
        {
            return 0;
        }

        public void Open(string path, string password)
        {
            _archivePath = path;
            _drive = new FileSystemItem
            {
                Name = Path.GetFileName(_archivePath),
                Path = string.Empty,
                FullPath = string.Format(@"{0}:\", _archivePath),
                Type = ItemType.Drive,
                Thumbnail = ResourceManager.GetContentByteArray("/Resources/Items/16x16/package.png")
            };

            _archive = ArchiveFactory.Open(path, Options.LookForHeader);
            var isRar = _archive is RarArchive;
            Slash = isRar ? BACKSLASH : SLASH;
            _fileStructure = new Tree<FileSystemItem> {Content = _drive};

            if (isRar)
            {
                try
                {
                    var c = _archive.Entries.Count();
                }
                catch (NullReferenceException ex)
                {
                    throw new PasswordProtectedException("Archive is corrupt or password protected!", ex);
                }
            }
            
            foreach (var entry in _archive.Entries.OrderBy(e => e.FilePath))
            {
                var entryPath = entry.FilePath;
                if (isRar) entryPath += BACKSLASH;
                _fileStructure.Insert(entryPath, CreateModel(entry), name => CreateModel(name, true, DateTime.MinValue, null));
            }
        }

        public override void Dispose()
        {
            _archive.Dispose();
            _archive = null;
            GC.Collect();
        }
    }
}