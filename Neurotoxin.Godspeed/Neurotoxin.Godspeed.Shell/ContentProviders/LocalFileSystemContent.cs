using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class LocalFileSystemContent : FileSystemContentBase
    {
        private readonly IWindowManager _windowManager;

        [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int WNetGetConnection(
            [MarshalAs(UnmanagedType.LPTStr)] string localName,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
            ref int length);

        public LocalFileSystemContent(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public override IList<FileSystemItem> GetDrives()
        {
            var drives = DriveInfo.GetDrives();
            var result = new List<FileSystemItem>();
            foreach (var drive in drives)
            {
                var name = drive.Name.TrimEnd('\\');
                string fullPath = null;
                var driveType = DriveType.Unknown;
                switch (drive.DriveType)
                {
                    case DriveType.CDRom:
                        driveType = drive.DriveType;
                        break;
                    case DriveType.Network:
                        driveType = drive.DriveType;
                        var sb = new StringBuilder(512);
                        var size = sb.Capacity;
                        if (WNetGetConnection(name, sb, ref size) == 0) fullPath = sb.ToString().TrimEnd();
                        break;
                    case DriveType.Removable:
                        driveType = drive.DriveType;
                        if (!drive.IsReady) continue;
                        break;
                }
                var item = new FileSystemItem
                {
                    Path = drive.Name,
                    FullPath = fullPath ?? drive.Name,
                    Name = name,
                    Type = ItemType.Drive,
                    DriveType = driveType
                };
                result.Add(item);
            }
            return result;
        }

        public override IList<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new NotSupportedException();
            var list = Directory.GetDirectories(path).Select(p => GetDirectoryInfo(p)).ToList();
            list.AddRange(Directory.GetFiles(path).Select(GetFileInfo));
            return list;
        }

        public override FileSystemItem GetItemInfo(string path, ItemType? type, bool swallowException)
        {
            if (path.EndsWith("\\") && Directory.Exists(path)) return GetDirectoryInfo(path, type);
            if (File.Exists(path)) return GetFileInfo(path);
            path += Slash;
            return Directory.Exists(path) ? GetDirectoryInfo(path, type) : null;
        }

        private FileSystemItem GetDirectoryInfo(string path, ItemType? type = null)
        {
            if (!path.EndsWith("\\")) path += Slash;
            var dirInfo = new FileInfo(path);

            return new FileSystemItem
            {
                Name = Path.GetFileName(path.TrimEnd(Slash)),
                Type = type ?? (dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) ? ItemType.Link : ItemType.Directory),
                Date = dirInfo.LastWriteTime,
                Path = path,
                FullPath = path
            };
        }

        private static FileSystemItem GetFileInfo(string path)
        {
            var fileInfo = new FileInfo(path);
            return new FileSystemItem
            {
                Name = fileInfo.Name,
                Type = ItemType.File,
                Date = fileInfo.LastWriteTime,
                Path = path,
                FullPath = path,
                Size = fileInfo.Length,
            };
        }

        public override bool DriveIsReady(string drive)
        {
            var driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == drive);
            return driveInfo != null && driveInfo.IsReady;
        }

        public override FileExistenceInfo FileExists(string path)
        {
            var file = new FileInfo(path);
            if (file.Exists) return file.Length;
            return false;
        }

        public override bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public override void DeleteFolder(string path)
        {
            Directory.Delete(path);
        }

        public override void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public override void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);
        }

        public override FileSystemItem Rename(string path, string newName)
        {
            var oldName = Path.GetFileName(path.TrimEnd(Slash));
            var r = new Regex(string.Format(@"{0}\\?$", Regex.Escape(oldName)), RegexOptions.IgnoreCase);
            var newPath = r.Replace(path, newName); 
            
            if (FolderExists(path))
            {
                try
                {
                    var newPath2 = newPath + Slash;
                    if (!string.Equals(path, newPath2))
                    {
                        var tmpPath = path;
                        if (string.Equals(path, newPath2, StringComparison.InvariantCultureIgnoreCase))
                        {
                            tmpPath = r.Replace(path, new Guid().ToString());
                            Directory.Move(path, tmpPath);
                        }
                        Directory.Move(tmpPath, newPath2);
                    }
                    return GetDirectoryInfo(newPath);
                }
                catch (Exception ex)
                {
                    _windowManager.ShowMessage(Resx.IOError, ex.Message);
                    return GetDirectoryInfo(path);
                }
            }
            else
            {
                try
                {
                    if (!string.Equals(path, newPath))
                    {
                        File.Move(path, newPath);
                    }
                    return GetFileInfo(newPath);
                }
                catch (Exception ex)
                {
                    _windowManager.ShowMessage(Resx.IOError, ex.Message);
                    return GetFileInfo(path);
                }
            }
        }

        public override Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition)
        {
            var stream = new FileStream(path, mode, access);
            if (startPosition != 0) stream.Seek(startPosition, SeekOrigin.Begin);
            return stream;
        }

        public override long? GetFreeSpace(string drive)
        {
            var driveInfo = DriveInfo.GetDrives().First(d => d.Name == drive);
            return driveInfo.AvailableFreeSpace;
        }
    }
}