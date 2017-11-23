using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Helpers;
using Neurotoxin.Godspeed.Core.Io.God;
using Neurotoxin.Godspeed.Core.Io.Iso;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Properties;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class IsoContent : FileSystemContentBase
    {
        private FileSystemItem _drive;
        private readonly SHA1Managed _sha1 = new SHA1Managed();

        private Xiso _xiso;
        public string Path { get; private set; }

        public XisoDetails Details
        {
            get { return _xiso.Details; }
        }

        public event ProgressChangedEventHandler ProgressChanged;

        private void OnProgressChanged(string message)
        {
            var handler = ProgressChanged;
            if (handler != null) handler(this, new ProgressChangedEventArgs(-1, message));
        }

        private void OnProgressChanged(int percentage)
        {
            var handler = ProgressChanged;
            if (handler != null) handler(this, new ProgressChangedEventArgs(percentage, null));
        }

        public override IList<FileSystemItem> GetDrives()
        {
            _drive = new FileSystemItem
            {
                Name = System.IO.Path.GetFileName(Path),
                Path = @"\",
                FullPath = string.Format(@"{0}:\", Path),
                Type = ItemType.Drive,
                Thumbnail = ResourceManager.GetContentByteArray("/Resources/Items/16x16/iso.png")
            };
            return new List<FileSystemItem> { _drive };
        }

        public override IList<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new NotSupportedException();
            return _xiso.GetDirContents(path).Select(CreateModel).ToList();
        }

        public override FileSystemItem GetItemInfo(string path, ItemType? type, bool swallowException)
        {
            if (path == _drive.Path) return _drive;
            var entry = GetEntry(path);
            if (entry == null) return null;

            var item = CreateModel(entry);
            if (type != null)
            {
                if ((type == ItemType.File && item.Type != ItemType.File) ||
                    (type != ItemType.File && item.Type != ItemType.Directory)) return null;
                item.Type = type.Value;
            }
            return item;
        }

        private XisoFileEntry GetEntry(string path)
        {
            return _xiso.GetEntry(path);
        }

        private FileSystemItem CreateModel(XisoFileEntry f)
        {
            var isDir = f.IsDirectory;
            var path = f.Path;
            return new FileSystemItem
            {
                Name = f.Name,
                Type = isDir ? ItemType.Directory : ItemType.File,
                Path = path,
                FullPath = string.Format(@"{0}:\{1}", Path, path),
                Size = isDir ? null : f.Size
            };
        }

        public override bool DriveIsReady(string drive)
        {
            return true;
        }

        public override FileExistenceInfo FileExists(string path)
        {
            var entry = GetEntry(path);
            if (entry == null) return false;
            return entry.Size;
        }

        public override bool FolderExists(string path)
        {
            if (path == _drive.Path) return true;
            var entry = GetEntry(path);
            return entry != null && entry.IsDirectory;
        }

        public override void DeleteFolder(string path)
        {
            throw new NotImplementedException();
        }

        public override void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public override void CreateFolder(string path)
        {
            throw new NotImplementedException();
        }

        public override FileSystemItem Rename(string path, string newName)
        {
            throw new NotImplementedException();
        }

        public byte[] GetFileContent(string path)
        {
            return _xiso.GetFile(path);
        }

        public override Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition)
        {
            var stream = new MemoryStream(GetFileContent(path));
            if (startPosition != 0) stream.Seek(startPosition, SeekOrigin.Begin);
            return stream;
        }

        public override long? GetFreeSpace(string drive)
        {
            return 0;
        }

        public void Load(string path)
        {
            Path = path;
            _xiso = new Xiso(path);
        }

        public override void Dispose()
        {
            if (_xiso == null) return;
            _xiso.Dispose();
            _xiso = null;
            GC.Collect();
        }

        public void Rebuild(string targetPath, bool skipSystemUpdate)
        {
            _xiso.Rebuild(targetPath, skipSystemUpdate);
        }

        public void ConvertToGod(string targetPath, string name, bool crop)
        {
            var size = crop ? _xiso.RealSize : _xiso.VolumeSize;
            long pos = 0;
            var dataBlockCount = (int)Math.Ceiling(((double)size) / 0x1000);
            var dataFileCount = (int)Math.Ceiling(((double)dataBlockCount) / 0xa1c4);
            var contentType = _xiso.ContentType;
            var uniqueName = GetUniqueName(_xiso);
            var path = System.IO.Path.Combine(targetPath, _xiso.Details.TitleId, ((int)contentType).ToString("X8"), uniqueName);

            var blockIndex = 0;
            long godSize = 0;
            _xiso.Seek(0);
            var stack = new Stack<DataFile>();
            HashTable lastTable = null;

            //write parts
            for (var i = 0; i < dataFileCount; i++)
            {
                OnProgressChanged(string.Format(Resources.WritingPart + Strings.DotDotDot, i+1, dataFileCount));

                Directory.CreateDirectory(path + ".data");
                var dataPath = string.Format("{0}.data\\Data{1:D4}", path, i);
                var f = new DataFile(dataPath);
                for (var j = 0; j < 0xcb; j++) // subhashtable per masterhashtable
                {
                    var table = ModelFactory.GetModel<HashTable>(new byte[DataFile.BlockLength]);
                    f.WriteBlankBlock();
                    long blockPerTable = 0;
                    while ((blockIndex < dataBlockCount) && (blockPerTable < 0xcc)) // block per subhashtable
                    {
                        var buffer = _xiso.ReadBytes(DataFile.BlockLength);
                        pos += buffer.Length;
                        if (buffer.Length < DataFile.BlockLength) Array.Resize(ref buffer, DataFile.BlockLength);
                        var blockHash = _sha1.ComputeHash(buffer, 0, DataFile.BlockLength);
                        table.Entries[blockPerTable].BlockHash = blockHash;
                        f.Write(buffer);
                        blockIndex++;
                        blockPerTable++;
                    }
                    OnProgressChanged((int)(pos * 100 / size));
                    var position = f.Position;
                    var tableBytes = table.Binary.ReadAll();
                    f.Seek(0 - ((blockPerTable + 1) * DataFile.BlockLength), SeekOrigin.Current);
                    f.Write(tableBytes);
                    f.Seek(position, SeekOrigin.Begin);
                    var masterHash = _sha1.ComputeHash(tableBytes, 0, DataFile.BlockLength);
                    f.SetBlockHash(j, masterHash);
                    if (blockIndex >= dataBlockCount) break;
                }
                if (blockIndex >= dataBlockCount)
                {
                    lastTable = f.MasterHashTable;
                    f.WriteMasterHashBlock();
                    godSize = f.Length;
                    f.Close();
                    break;
                }
                stack.Push(f);
            }

            //calculate mht hash chain
            OnProgressChanged(Resources.CalculatingMasterHashTablechain + Strings.DotDotDot);
            foreach (var f in stack)
            {
                var tableBytes = lastTable.Binary.ReadAll();
                var masterHash = _sha1.ComputeHash(tableBytes, 0, DataFile.BlockLength);
                f.SetBlockHash(0xcb, masterHash);
                f.WriteMasterHashBlock();
                godSize += f.Length;
                f.Close();
                lastTable = f.MasterHashTable;
            }

            //create con header
            OnProgressChanged(Resources.CreatingHeaderFile + Strings.DotDotDot);
            var conHeader = ResourceHelper.GetEmptyConHeader();
            conHeader.TitleId = _xiso.Details.TitleId;
            conHeader.MediaId = _xiso.Details.MediaId;
            conHeader.Version = _xiso.Details.Version;
            conHeader.BaseVersion = _xiso.Details.BaseVersion;
            conHeader.DisplayName = name;
            conHeader.TitleName = name;
            conHeader.Platform = _xiso.Details.Platform;
            conHeader.ExecutableType = _xiso.Details.ExecutionType;
            conHeader.DiscNumber = _xiso.Details.DiscNumber;
            conHeader.DiscInSet = _xiso.Details.DiscCount;
            conHeader.VolumeDescriptor.DeviceFeatures = 0;
            conHeader.VolumeDescriptor.DataBlockCount = dataBlockCount;
            conHeader.VolumeDescriptor.DataBlockOffset = 0;
            conHeader.VolumeDescriptor.Hash = _sha1.ComputeHash(lastTable.Binary.ReadAll(), 0, DataFile.BlockLength);
            conHeader.DataFileCount = dataFileCount;
            conHeader.DataFileCombinedSize = godSize;
            conHeader.DescriptorType = VolumeDescriptorType.SVOD;
            conHeader.ThumbnailImage = _xiso.Details.Thumbnail;
            conHeader.ThumbnailImageSize = _xiso.Details.Thumbnail.Length;
            conHeader.TitleThumbnailImage = _xiso.Details.Thumbnail;
            conHeader.TitleThumbnailImageSize = _xiso.Details.Thumbnail.Length;
            conHeader.ContentType = _xiso.ContentType;

            //conHeader.Resign();
            conHeader.HeaderHash = conHeader.HashBlock(0x344, 0xacbc);
            conHeader.Save(path);
        }

        private string GetUniqueName(Xiso xiso)
        {
            var s = new MemoryStream();
            var writer = new BinaryWriter(s);
            writer.Write(xiso.Details.TitleId);
            writer.Write(xiso.Details.MediaId);
            writer.Write(xiso.Details.DiscNumber);
            writer.Write(xiso.Details.DiscCount);
            var buffer = _sha1.ComputeHash(s.ToArray());
            Array.Resize(ref buffer, buffer.Length / 2);
            return buffer.ToHex();
        }

    }
}