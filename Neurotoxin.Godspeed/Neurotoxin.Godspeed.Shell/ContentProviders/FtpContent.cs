using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Core.Net.Cryptography;
using Neurotoxin.Godspeed.Core.Security;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Helpers;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;
using Timer = System.Timers.Timer;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class FtpContent : FileSystemContentBase
    {
        private bool _isIdle = true;
        private readonly Timer _keepAliveTimer = new Timer(30000);
        public readonly FtpTraceListener TraceListener;

        public bool IsKeepAliveEnabled
        {
            get { return _keepAliveTimer.Enabled; }
            set { _keepAliveTimer.Enabled = value; }
        }

        public FtpConnection Connection { get; private set; }
        public FtpServerType ServerType { get; private set; }

        private FtpClient _ftpClient;
        private FtpClient FtpClient
        {
            get
            {
                if (_keepAliveTimer.Enabled)
                {
                    _keepAliveTimer.Stop();
                    _keepAliveTimer.Start();
                }
                return _ftpClient;
            }
        }

        public bool IsConnected
        {
            get { return FtpClient.IsConnected; }
        }

        public bool IsFSD
        {
            get { return ServerType == FtpServerType.F3 || ServerType == FtpServerType.FSD || ServerType == FtpServerType.IIS; }
        }

        public bool IsAurora
        {
            get { return ServerType == FtpServerType.Aurora || ServerType == FtpServerType.AuroraAlpha; }
        }

        public bool IsUTF8Supported
        {
            get { return FtpClient.Capabilities.HasFlag(FtpCapability.UTF8); }
        }

        public bool HasWebUI
        {
            get { return (ServerType == FtpServerType.F3 || IsAurora) && !Connection.IsHttpAccessDisabled; }
        }

        public Stack<string> Log
        {
            get { return TraceListener.Log; }
        }

        public FtpContent() : base('/')
        {
            _keepAliveTimer.AutoReset = true;
            _keepAliveTimer.Elapsed += KeepAliveTimerOnElapsed;
            TraceListener = new FtpTraceListener();
        }

        private void KeepAliveTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_isIdle) FtpClient.Execute("NOOP");
        }

        internal ResumeCapability Connect(FtpConnection connection, bool addTraceListener = true, EventHandler<FtpGetListingEventArgs> onGetListingDataReceived = null)
        {
            _ftpClient = new FtpClient
                {
                    EnableThreadSafeDataConnections = false,
                    DataConnectionType = connection.UsePassiveMode ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT,
                    Host = connection.Address, 
                    Port = connection.Port
                };
            if (addTraceListener) FtpTrace.AddListener(TraceListener);

            Connection = connection;
            _ftpClient.BeforeAuthentication += OnBeforeAuthentication;
            _ftpClient.Connected += OnConnected;
            if (onGetListingDataReceived != null) _ftpClient.GetListingDataReceived += onGetListingDataReceived;
            _ftpClient.Connect();

            var resume = ResumeCapability.None;
            var r = FtpClient.Execute("APPE");
            if (!r.Message.Contains("command not recognized")) resume |= ResumeCapability.Append;

            r = FtpClient.Execute("REST");
            if (!r.Message.Contains("command not recognized")) resume |= ResumeCapability.Restart;
            return resume;
        }

        private void OnBeforeAuthentication(object sender, EventArgs eventArgs)
        {
            var ftp = (FtpClient) sender;

            FtpServerType type;
            EnumHelper.TryGetField(_ftpClient.ServerName.Trim(), out type);
            ServerType = type;

            if (string.IsNullOrEmpty(Connection.Username))
            {
                switch (type)
                {
                    case FtpServerType.Aurora:
                        ftp.Credentials = new NetworkCredential("xboxftp", "xboxftp");
                        break;
                    case FtpServerType.MinFTPD:
                    case FtpServerType.FSD:
                    case FtpServerType.F3:
                    case FtpServerType.XeXMenu:
                    case FtpServerType.DashLaunch:
                    case FtpServerType.FtpDll:
                        ftp.Credentials = new NetworkCredential("xbox", "xbox");
                        break;
                    default:
                        ftp.Credentials = new NetworkCredential("anonymous", "no@email.com");
                        break;
                }
            }
            else
            {
                ftp.Credentials = new NetworkCredential(Connection.Username, Connection.Password);    
            }

            //override user settings in case of these server types
            switch (ServerType)
            {
                case FtpServerType.Aurora:
                    _ftpClient.DataConnectionType = FtpDataConnectionType.PASV;
                    break;
                case FtpServerType.XeXMenu:
                case FtpServerType.DashLaunch:
                    _ftpClient.DataConnectionType = FtpDataConnectionType.PORT;
                    break;
            }
        }

        private void OnConnected(object sender, EventArgs e)
        {
            FtpReply r;
            if (ServerType == FtpServerType.Aurora)
            {
                r = FtpClient.Execute("HELP SITE");
                if (!r.Success) ServerType = FtpServerType.AuroraAlpha;
            }
            if (!_ftpClient.Capabilities.HasFlag(FtpCapability.SIZE)) return;
            r = FtpClient.Execute("SIZE");
            if (r.Message.Contains("command not recognized") || r.Message.Contains("command not implemented"))
            {
                _ftpClient.Capabilities &= ~FtpCapability.SIZE;
            }
        }

        internal void Disconnect(EventHandler<FtpGetListingEventArgs> onGetListingDataReceived = null)
        {
            FtpClient.Connected -= OnConnected;
            if (onGetListingDataReceived != null) FtpClient.GetListingDataReceived -= onGetListingDataReceived;
            FtpTrace.RemoveListener(TraceListener);
            _keepAliveTimer.Elapsed -= KeepAliveTimerOnElapsed;
            FtpClient.Dispose();
        }

        internal void Shutdown()
        {
            switch (ServerType)
            {
                case FtpServerType.FSD:
                case FtpServerType.F3:
                    FtpClient.Execute("SHUTDOWN");
                    break;
                case FtpServerType.Aurora:
                    FtpClient.Execute("SITE SHUTDOWN");
                    break;
                case FtpServerType.FtpDll:
                    FtpClient.Execute("SHDN");
                    break;
            }
            //TODO: default throw exception?
        }

        public override IList<FileSystemItem> GetDrives()
        {
            List<FileSystemItem> result;
            NotifyTransferStarted(false);
            try
            {
                var currentFolder = FtpClient.GetWorkingDirectory();
                FtpClient.SetWorkingDirectory("/");

                result = FtpClient.GetListing()
                                  .Select(item => new FileSystemItem
                                      {
                                          Name = item.Name,
                                          Type = ItemType.Drive,
                                          Date = item.Modified,
                                          Path = string.Format("/{0}/", item.Name),
                                          FullPath = string.Format("{0}://{1}/", Connection.Name, item.Name)
                                      })
                                  .ToList();
                FtpClient.SetWorkingDirectory(currentFolder);
            }
            finally
            {
                NotifyTransferFinished();
            }
            return result;
        }

        public override IList<FileSystemItem> GetList(string path = null)
        {

            List<FileSystemItem> result;
            NotifyTransferStarted(false);
            try
            {
                var currentPath = path;
                if (path != null)
                    FtpClient.SetWorkingDirectory(path);
                else
                    currentPath = FtpClient.GetWorkingDirectory();
                if (!currentPath.EndsWith("/")) currentPath += "/";
                try
                {
                    result = FtpClient.GetListing()
                                      .Select(item => CreateModel(item, string.Format("{0}{1}{2}", currentPath, item.Name, item.Type == FtpFileSystemObjectType.Directory ? "/" : string.Empty)))
                                      .ToList();
                }
                catch (Exception ex)
                {
                    //XeXMenu throws an exception if the folder is empty (dull)
                    if (ServerType == FtpServerType.XeXMenu && ex.Message.Contains("Path not found")) return new List<FileSystemItem>();
                    throw;
                }
            }
            finally
            {
                NotifyTransferFinished();
            }
            return result;
        }

        private string LocateDirectory(string path)
        {
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            var filename = path.Replace(dir, String.Empty);
            FtpClient.SetWorkingDirectory(dir);
            return filename;
        }

        public override FileSystemItem GetItemInfo(string path, ItemType? type, bool swallowException)
        {
            NotifyTransferStarted(false);
            try
            {
                var itemName = LocateDirectory(path);
                var item = FtpClient.GetListing().FirstOrDefault(i => i.Name == itemName);
                NotifyTransferFinished();
                if (item == null) return null;
                if (type != null)
                {
                    FtpFileSystemObjectType ftpItemType;
                    switch (type)
                    {
                        case ItemType.File:
                            ftpItemType = FtpFileSystemObjectType.File;
                            break;
                        case ItemType.Directory:
                        case ItemType.Drive:
                            ftpItemType = FtpFileSystemObjectType.Directory;
                            break;
                        case ItemType.Link:
                            ftpItemType = FtpFileSystemObjectType.Link;
                            break;
                        default:
                            throw new NotSupportedException("Invalid item type:" + type);
                    }
                    if (item.Type != ftpItemType) return null;
                }
                var m = CreateModel(item, path);
                if (type != null) m.Type = type.Value;
                return m;
            }
            catch
            {
                NotifyTransferFinished();
                if (swallowException) return null;
                throw;
            }
        }

        private FileSystemItem CreateModel(IFtpListItem item, string path)
        {
            if (item.Type == FtpFileSystemObjectType.Directory && !path.EndsWith("/")) path += "/";
            ItemType type;
            switch (item.Type)
            {
                case FtpFileSystemObjectType.File:
                    type = ItemType.File;
                    break;
                case FtpFileSystemObjectType.Directory:
                    type = ItemType.Directory;
                    break;
                case FtpFileSystemObjectType.Link:
                    type = ItemType.Link;
                    break;
                default:
                    throw new NotSupportedException("Invalid FTP item type: " + item.Type);
            }
            return new FileSystemItem
                       {
                           Name = item.Name,
                           Type = type,
                           Date = item.Modified,
                           Path = path,
                           FullPath = string.Format("{0}:/{1}", Connection.Name, path),
                           Size = item.Type == FtpFileSystemObjectType.Directory ? null : (long?)item.Size
                       };
        }

        public override bool DriveIsReady(string drive)
        {
            try
            {
                FtpClient.SetWorkingDirectory(drive);
                return true;
            }
            catch (FtpException)
            {
                if (!FtpClient.IsConnected) throw;
                return false;
            }
        }

        public override FileExistenceInfo FileExists(string path)
        {
            NotifyTransferStarted(false);
            try
            {
                var filename = LocateDirectory(path);
                return FtpClient.GetFileSize(filename);
            }
            catch {}
            NotifyTransferFinished();
            return false;
        }

        public override bool FolderExists(string path)
        {
            return FtpClient.DirectoryExists(path);
        }

        public override void DeleteFolder(string path)
        {
            //FtpClient.ClearListingCache();
            FtpClient.DeleteDirectory(ServerType == FtpServerType.MinFTPD ? LocateDirectory(path) : path);
        }

        public override void DeleteFile(string path)
        {
            //FtpClient.ClearListingCache();
            FtpClient.DeleteFile(ServerType == FtpServerType.MinFTPD ? LocateDirectory(path) : path);
        }

        public override void CreateFolder(string path)
        {
            //FtpClient.ClearListingCache();
            FtpClient.CreateDirectory(ServerType == FtpServerType.MinFTPD ? LocateDirectory(path) : path);
        }

        public override FileSystemItem Rename(string path, string newName)
        {
            //FtpClient.ClearListingCache();
            var oldName = LocateDirectory(path);
            FtpClient.Rename(oldName, newName);
            var r = new Regex(string.Format("{0}{1}?$", Regex.Escape(oldName), Slash), RegexOptions.IgnoreCase);
            path = r.Replace(path, newName);
            var item = GetItemInfo(path);
            if (item == null) throw new ApplicationException(string.Format(Resx.ItemNotExistsOnPath, path));
            return item;
        }

        public override Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition)
        {
            var filename = LocateDirectory(path);
            switch (access)
            {
                case FileAccess.Read:
                    return FtpClient.OpenRead(filename, startPosition);
                case FileAccess.Write:
                    return mode == FileMode.Append ? FtpClient.OpenAppend(filename) : FtpClient.OpenWrite(filename);
                default:
                    throw new NotSupportedException("Invalid FileAccess: " + access);
            }
        }

        protected override bool HandleCopyStreamExceptions(Exception ex, bool isAborted)
        {
            if (ex is FtpException && isAborted)
            {
                switch (ServerType)
                {
                    case FtpServerType.Aurora:
                        return ex.Message == "Connection interrupted";
                    case FtpServerType.FSD:
                    case FtpServerType.F3:
                        FtpClient.ClearStaleData();
                        return true;
                }
            }
            return false;
        }

        protected override void AbortStream(Stream stream)
        {
            FtpClient.ClearListingCache();
            //TODO: Test every server type
            if (ServerType == FtpServerType.IIS) FtpClient.Execute("ABOR");
            //FSD3 doesn't require ABOR
        }

        protected override void NotifyTransferStarted(bool binaryTransfer)
        {
            _isIdle = false;
            base.NotifyTransferStarted(binaryTransfer);
        }

        protected override void NotifyTransferFinished(long? streamLength = null)
        {
            base.NotifyTransferFinished(streamLength);
            _isIdle = true;
        }

        public override long? GetFreeSpace(string drive)
        {
            if (ServerType != FtpServerType.Aurora) return null;

            //HACK: Aurora changes current work directory after calling SITE DRIVEBYTESFREE
            var currentDir = FtpClient.CurrentWorkingDirectory;
            var reply = FtpClient.Execute("SITE DRIVEBYTESFREE");
            FtpClient.Execute("CWD " + currentDir);
            long freespace;
            if (long.TryParse(reply.Message, out freespace)) return freespace;
            return null;
        }

        public void RestoreConnection()
        {
            Disconnect();
            Connect(Connection, false);
        }

        public bool Verify(string remotePath, string localPath)
        {
            if (FileExists(remotePath) != new FileInfo(localPath).Length) return false;

            if (FtpClient.Capabilities.HasFlag(FtpCapability.XCRC))
            {
                var readTimeout = FtpClient.ReadTimeout;
                FtpClient.ReadTimeout = Timeout.Infinite;
                var remoteChecksum = FtpClient.GetXCRC(remotePath);
                FtpClient.ReadTimeout = readTimeout;
                var crc = new Crc32();
                using (var f = new FileStream(localPath, FileMode.Open, FileAccess.Read))
                {
                    var bytes = crc.ComputeHash(f);
                    var sb = new StringBuilder();
                    foreach (var b in bytes)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    return sb.ToString().Equals(remoteChecksum, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            return false;
        }

        public void Execute(string path)
        {
            switch (ServerType)
            {
                case FtpServerType.Aurora:
                    FtpClient.Execute("SITE LAUNCH " + path);
                    break;
                case FtpServerType.FSD:
                case FtpServerType.F3:
                case FtpServerType.FtpDll:
                    FtpClient.Execute("EXEC " + path);
                    break;
                default:
                    throw new NotSupportedException("Invalid server type: " + ServerType);
            }
        }

        public Dictionary<string, string> StorageInfo()
        {
            var reply = FtpClient.Execute("SITE STORAGEINFO");
            return reply.InfoMessages.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToDictionary(row => row.SubstringBefore(':').Trim(), row => row.SubstringAfter(':').Trim());
        }

        public bool UploadFile(string targetPath, string sourcePath)
        {
            NotifyTransferStarted(true);
            FileStream fs = null;
            var uploadedStarted = false;
            long fileSize = 0;
            long transferred = 0;
            var bufferSize = 0;
            try
            {
                _isIdle = false;
                IsAborted = false;
                fileSize = new FileInfo(sourcePath).Length;
                fs = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
                var filename = LocateDirectory(targetPath);
                using (var ftpStream = FtpClient.OpenWrite(filename))
                {
                    uploadedStarted = true;
                    var buffer = new byte[0x8000];
                    while (!IsAborted && (bufferSize = fs.Read(buffer, 0, 0x8000)) > 0)
                    {
                        ftpStream.Write(buffer, 0, bufferSize);
                        transferred += bufferSize;
                        NotifyTransferProgressChanged(fileSize, bufferSize, transferred, 0);
                    }
                }
            }
            catch
            {
                if (uploadedStarted) NotifyTransferProgressChanged(fileSize, bufferSize, transferred, 0, true);
                throw;
            }
            finally
            {
                NotifyTransferFinished();
                if (fs != null) fs.Close();
            }
            return !IsAborted;
        }

        public void ClearListingCache()
        {
            FtpClient.ClearListingCache();
        }
    }
}