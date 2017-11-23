using System;
using System.Collections.Generic;
using System.IO;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IFileManager : IDisposable
    {
        char Slash { get; }

        IList<FileSystemItem> GetDrives();
        IList<FileSystemItem> GetList(string path = null);

        FileSystemItem GetItemInfo(string itemPath);
        FileSystemItem GetItemInfo(string itemPath, ItemType? type);
        FileSystemItem GetItemInfo(string itemPath, ItemType? type, bool swallowException);

        bool DriveIsReady(string drive);
        FileExistenceInfo FileExists(string path);
        bool FolderExists(string path);

        void DeleteFolder(string path);
        void DeleteFile(string path);

        void CreateFolder(string path);

        byte[] ReadFileContent(string path);
        byte[] ReadFileContent(string path, long byteLimit);

        FileSystemItem Rename(string path, string newName);

        Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition);
        bool CopyStream(FileSystemItem item, Stream stream, long startPosition = 0, long? byteLimit = null);
        void Abort();
        long? GetFreeSpace(string drive);
    }
}