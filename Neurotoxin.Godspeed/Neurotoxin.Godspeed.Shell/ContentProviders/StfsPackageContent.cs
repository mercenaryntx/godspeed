using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class StfsPackageContent : FileSystemContentBase
    {
        private ContentType _contentType;
        private StfsPackage _stfs;

        public override IList<FileSystemItem> GetDrives()
        {
            const string path = @"\Root\";
            return new List<FileSystemItem>
                       {
                           new FileSystemItem
                               {
                                   Name = _stfs.DisplayName,
                                   Path = path,
                                   FullPath = string.Format(@"{0}:\{1}", _stfs.DisplayName, path),
                                   Type = ItemType.Drive,
                                   Thumbnail = _stfs.ThumbnailImage,
                                   ContentType = _contentType
                               }
                       };
        }

        public override IList<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new NotSupportedException();

            var folder = _stfs.GetFolderEntry(path);
            var list = folder.Folders.Select(f => CreateModel(f, string.Format(@"{0}{1}\", path, f.Name))).ToList();
            list.AddRange(folder.Files.Select(f => CreateModel(f, string.Format(@"{0}{1}", path, f.Name))));

            return list;
        }

        public override FileSystemItem GetItemInfo(string path, ItemType? type, bool swallowException)
        {
            var item = GetFileInfo(path, true) ?? GetFolderInfo(path);
            if (item == null) return null;
            if (type != null)
            {
                if ((type == ItemType.File && item.Type != ItemType.File) || 
                    (type != ItemType.File && item.Type != ItemType.Directory)) return null;
                item.Type = type.Value;
            }
            return item;
        }

        private FileSystemItem GetFolderInfo(string path)
        {
            if (!path.EndsWith("\\")) path += "\\";
            return CreateModel(_stfs.GetFolderEntry(path), path);
        }

        public FileSystemItem GetFileInfo(string path, bool allowNull = false)
        {
            var f = _stfs.GetFileEntry(path, allowNull);
            return f == null ? null : CreateModel(f, path);
        }

        private FileSystemItem CreateModel(FileEntry f, string path)
        {
            return new FileSystemItem
            {
                Name = f.Name,
                Type = f.IsDirectory ? ItemType.Directory : ItemType.File,
                Path = path,
                FullPath = string.Format(@"{0}:\{1}", _stfs.DisplayName, path),
                Date = DateTimeExtensions.FromFatFileTime(f.AccessTimeStamp),
                Size = f.FileSize
            };
        }

        public override bool DriveIsReady(string drive)
        {
            return true;
        }

        public override FileExistenceInfo FileExists(string path)
        {
            var entry = _stfs.GetFileEntry(path, true);
            if (entry == null) return false;
            return entry.FileSize;
        }

        public override bool FolderExists(string path)
        {
            return _stfs.GetFolderEntry(path, true) != null;
        }

        public override void DeleteFolder(string path)
        {
            _stfs.RemoveFolder(path);
        }

        public override void DeleteFile(string path)
        {
            _stfs.RemoveFile(path);
        }

        public override void CreateFolder(string path)
        {
            _stfs.AddFolder(path);
        }

        //public void ExtractFile(string remotePath, FileStream fs, long remoteStartPosition)
        //{
        //    _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(0, 0, 0, 0));
        //    var bytes = ReadFileContent(remotePath);
        //    var offset = (int)remoteStartPosition;
        //    fs.Write(bytes, offset, bytes.Length - offset);
        //    _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(100, bytes.Length, bytes.Length, 0));
        //}

        //public void AddFile(string targetPath, string sourcePath)
        //{
        //    _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(0, 0, 0, 0));
        //    var content = File.ReadAllBytes(sourcePath);
        //    _stfs.AddFile(targetPath, content);
        //    _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(100, content.Length, content.Length, 0));
        //}

        //public void ReplaceFile(string targetPath, string sourcePath)
        //{
        //    _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(0, 0, 0, 0));
        //    var content = File.ReadAllBytes(sourcePath);
        //    var fileEntry = _stfs.GetFileEntry(targetPath);
        //    _stfs.ReplaceFile(fileEntry, content);
        //    _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(100, content.Length, content.Length, 0));
        //}

        public override FileSystemItem Rename(string path, string newName)
        {
            var entry = _stfs.Rename(path, newName);
            var oldName = Path.GetFileName(path.TrimEnd('\\'));
            var r = new Regex(string.Format(@"{0}\\?$", Regex.Escape(oldName)), RegexOptions.IgnoreCase);
            var newPath = r.Replace(path, newName);
            return CreateModel(entry, newPath);
        }

        public override Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition)
        {
            var stream = _stfs.OpenEntryStream(path);
            if (startPosition != 0) stream.Seek(startPosition, SeekOrigin.Begin);
            return stream;
        }

        public override long? GetFreeSpace(string drive)
        {
            //TODO: calc unallocated blocks
            return 0;
        }

        public Account GetAccount()
        {
            if (_stfs.Account == null) _stfs.ExtractAccount();
            return _stfs.Account;
        }

        public void LoadPackage(BinaryContent content)
        {
            _contentType = content.ContentType;
            _stfs = ModelFactory.GetModel<StfsPackage>(content.Content);
        }

        public byte[] Save()
        {
            return _stfs.Save();
        }


        public override void Dispose()
        {
            _stfs = null;
            GC.Collect();
        }
    }
}