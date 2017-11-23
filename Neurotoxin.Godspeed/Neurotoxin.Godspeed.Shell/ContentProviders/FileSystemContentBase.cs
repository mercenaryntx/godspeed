using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public abstract class FileSystemContentBase : IFileManager
    {
        protected bool IsAborted;
        private readonly Stopwatch _notificationTimer = new Stopwatch();
        private long _aggregatedTransferredValue;

        protected readonly IUnityContainer Container;
        protected readonly IEventAggregator EventAggregator;
        protected readonly IResourceManager ResourceManager;

        public char Slash { get; protected set; }

        public FileSystemContentBase() : this('\\')
        {
        }

        public FileSystemContentBase(char slash)
        {
            Slash = slash;
            Container = UnityInstance.Container;
            EventAggregator = Container.Resolve<IEventAggregator>();
            ResourceManager = Container.Resolve<IResourceManager>();
        }

        public abstract IList<FileSystemItem> GetDrives();
        public abstract IList<FileSystemItem> GetList(string path = null);
        public abstract FileSystemItem GetItemInfo(string path, ItemType? type, bool swallowException);
        public abstract bool DriveIsReady(string drive);
        public abstract FileExistenceInfo FileExists(string path);
        public abstract bool FolderExists(string path);
        public abstract void DeleteFolder(string path);
        public abstract void DeleteFile(string path);
        public abstract void CreateFolder(string path);
        public abstract FileSystemItem Rename(string path, string newName);
        public abstract Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition);
        public abstract long? GetFreeSpace(string drive);
        
        public virtual FileSystemItem GetItemInfo(string path)
        {
            return GetItemInfo(path, null);
        }

        public virtual FileSystemItem GetItemInfo(string path, ItemType? type)
        {
            return GetItemInfo(path, type, true);
        }

        public virtual byte[] ReadFileContent(string path)
        {
            return ReadFileContent(path, -1);
        }

        public virtual byte[] ReadFileContent(string path, long byteLimit)
        {
            var ms = new MemoryStream();
            if (byteLimit <= 0)
            {
                var exists = FileExists(path);
                if (!exists) throw new IOException("File not exists: " + path);
                byteLimit = exists.Size;
            }
            CopyStream(path, ms, 0, byteLimit);
            var result = ms.ToArray();
            ms.Close();
            return result;
        }

        public bool CopyStream(FileSystemItem item, Stream stream, long startPosition = 0, long? byteLimit = null)
        {
            return CopyStream(item.Path, stream, startPosition, byteLimit ?? item.Size ?? FileExists(item.Path));
        }

        protected virtual bool CopyStream(string path, Stream stream, long startPosition, long byteLimit)
        {
            var copyStarted = false;
            long transferred = 0;
            var bufferSize = 0;
            NotifyTransferStarted(true);
            try
            {
                IsAborted = false;
                if (startPosition != 0) NotifyTransferResume((int)(startPosition * 100 / byteLimit), startPosition);

                using (var sourceStream = GetStream(path, FileMode.Open, FileAccess.Read, startPosition))
                {
                    copyStarted = true;
                    var buffer = new byte[0x8000];
                    while (!IsAborted && (bufferSize = sourceStream.Read(buffer, 0, 0x8000)) > 0)
                    {
                        if (transferred + bufferSize > byteLimit)
                        {
                            bufferSize = (int)(byteLimit - transferred);
                            IsAborted = true;
                        }
                        stream.Write(buffer, 0, bufferSize);
                        transferred += bufferSize;
                        NotifyTransferProgressChanged(byteLimit, bufferSize, transferred, startPosition);
                    }
                    if (IsAborted) AbortStream(sourceStream);
                }
                stream.Flush();
                NotifyTransferFinished(stream.Length);
            }
            catch (Exception ex)
            {
                if (copyStarted) NotifyTransferProgressChanged(byteLimit, bufferSize, transferred, startPosition, true);
                NotifyTransferFinished();
                if (!HandleCopyStreamExceptions(ex, IsAborted)) throw;
            }
            return !IsAborted;
        }

        protected virtual bool HandleCopyStreamExceptions(Exception ex, bool isAborted)
        {
            return false;
        }

        protected virtual void AbortStream(Stream stream)
        {
            
        }

        protected virtual void NotifyTransferResume(int percentage, long resumeStartPosition)
        {
            EventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(percentage, resumeStartPosition, resumeStartPosition, resumeStartPosition));
        }

        protected virtual void NotifyTransferStarted(bool binaryTransfer)
        {
            EventAggregator.GetEvent<FtpOperationStartedEvent>().Publish(new FtpOperationStartedEventArgs(binaryTransfer));
            _notificationTimer.Restart();
        }

        protected virtual void NotifyTransferFinished(long? streamLength = null)
        {
            EventAggregator.GetEvent<FtpOperationFinishedEvent>().Publish(new FtpOperationFinishedEventArgs(streamLength));
            _notificationTimer.Stop();
        }

        protected virtual void NotifyTransferProgressChanged(long fileSize, long transferred, long totalBytesTransferred, long resumeStartPosition, bool force = false)
        {
            _aggregatedTransferredValue += transferred;
            var percentage = (int)((resumeStartPosition + totalBytesTransferred) * 100 / fileSize);
            if (!force && _notificationTimer.Elapsed.TotalMilliseconds < 100 && percentage != 100) return;

            EventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(percentage, _aggregatedTransferredValue, totalBytesTransferred, resumeStartPosition));

            _notificationTimer.Restart();
            _aggregatedTransferredValue = 0;
        }

        public void Abort()
        {
            IsAborted = true;
        }

        public virtual void Dispose()
        {
        }

    }
}