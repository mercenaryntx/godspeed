using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using HtmlAgilityPack;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Xtaf;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Extensions;
using Neurotoxin.Godspeed.Shell.Helpers;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using ServiceStack;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;
using Fizzler.Systems.HtmlAgilityPack;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FtpContentViewModel : FileListPaneViewModelBase<FtpContent>
    {
        private const string ProfileFilePattern = "/Content/{0}/FFFE07D1/00010000/{0}/$";

        private readonly HashSet<int> _doContentScanOn = new HashSet<int>();
        private readonly Dictionary<string, string> _driveLabelCache = new Dictionary<string, string>();
        private string _fsdStatus;
        private readonly Timer _statusUpdateTimer = new Timer(3000);

        public FtpConnectionItemViewModel Connection { get; private set; }
        public Stack<string> Log { get { return FileManager.Log; } }
        public Dictionary<int, FsdScanPath> ScanFolders { get; private set; }
        public string HttpSessionId { get; private set; }

        private const string ISCONNECTED = "IsConnected";
        public bool IsConnected
        {
            get { return FileManager.IsConnected; }
        }

        public bool IsKeepAliveEnabled
        {
            get { return FileManager.IsKeepAliveEnabled; }
            set { FileManager.IsKeepAliveEnabled = value; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override bool IsFavoritesSupported
        {
            get { return true; }
        }

        private const string ISCONTENTSCANTRIGGERAVAILABLE = "IsContentScanTriggerAvailable";
        private bool IsContentScanTriggerAvailable
        {
            get
            {
                return UserSettingsProvider.FsdContentScanTrigger != FsdContentScanTrigger.Disabled &&
                       Connection != null && !Connection.IsHttpAccessDisabled && FileManager.IsFSD;
            }
        }

        private const string ISFSD = "IsFSD";
        public override bool IsFSD
        {
            get { return FileManager.IsFSD; }
        }

        private const string ISVERIFICATIONENABLED = "IsVerificationEnabled";
        public override bool IsVerificationEnabled
        {
            get { return UserSettingsProvider.VerifyFileHashAfterFtpUpload && FileManager.IsFSD; }
        }

        public override int? ConnectionId
        {
            get { return Connection.Model.Id; }
        }

        private const string SERVERTYPE = "ServerType";
        public FtpServerType ServerType
        {
            get { return FileManager.ServerType; }
        }

        private const string ISSHUTDOWNSUPPORTED = "IsShutdownSupported";
        public bool IsShutdownSupported
        {
            get { return IsFSD || ServerType == FtpServerType.Aurora || ServerType == FtpServerType.FtpDll; }
        }

        private const string ISLAUNCHSUPPORTED = "IsLaunchSupported";
        public bool IsLaunchSupported
        {
            get { return IsFSD || ServerType == FtpServerType.Aurora || ServerType == FtpServerType.FtpDll; }
        }

        #region Command overrides

        private void ExecuteCloseCommand()
        {
            EventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, Connection));
            Dispose();
        }

        protected override bool CanExecuteOpenCompressedFileCommand()
        {
            return false;
        }

        protected override void ExecuteChangeDirectoryCommand(object cmdParam)
        {
            if (CurrentRow != null && CurrentRow.IsXex && IsLaunchSupported)
            {
                if (ServerType == FtpServerType.FtpDll || WindowManager.Confirm(Resx.LaunchXex, Resx.LaunchXexConfirmation)) ExecuteLaunchCommand();
                return;
            }
            base.ExecuteChangeDirectoryCommand(cmdParam);
        }

        #endregion

        #region CheckFreestyleDatabaseCommand

        public DelegateCommand CheckFreestyleDatabaseCommand { get; private set; }

        public bool CanExecuteCheckFreestyleDatabaseCommand()
        {
            return FileManager.IsFSD;
        }

        public void ExecuteCheckFreestyleDatabaseCommand()
        {
            if (WindowManager.ActivateWindowOf<FreestyleDatabaseCheckerViewModel>()) return;
            var vm = Container.Resolve<FreestyleDatabaseCheckerViewModel>(new ParameterOverride("parent", this));
            vm.Check();
        }

        #endregion

        #region LaunchCommand

        public DelegateCommand LaunchCommand { get; private set; }

        public bool CanExecuteLaunchCommand()
        {
            if (CurrentRow == null) return false;
            if (CurrentRow.IsGame && FileManager.IsFSD) return true;
            if (CurrentRow.IsXex &&
                (FileManager.IsFSD || ServerType == FtpServerType.FtpDll || ServerType == FtpServerType.Aurora))
                return true;
            return false;
        }

        public void ExecuteLaunchCommand()
        {
            if (CurrentRow.IsGame)
            {
                LaunchErrorCode error;
                try
                {
                    var scanFolder = GetCorrespondingScanFolder(CurrentRow.Path);
                    if (scanFolder != null)
                    {
                        var prefix = CurrentRow.Path.Replace("/", "\\").SubstringAfter(scanFolder.Drive);
                        foreach (var row in GetContentItems())
                        {
                            if (Int32.Parse(row[FsdContentItemProperty.ScanPathId].Trim()) == scanFolder.PathId && 
                                row[FsdContentItemProperty.Directory].Trim().StartsWith(prefix))
                            {
                                var contentId = Int32.Parse(row[FsdContentItemProperty.Id]);
                                HttpPostAsync("launch", string.Format("sessionid={0}&contentid={1:X2}&Action=launch", HttpSessionId, contentId));
                                ExecuteCloseCommand();
                                return;
                            }
                        }
                        error = LaunchErrorCode.NotFound;
                    }
                    else
                    {
                        error = LaunchErrorCode.NotUnderScanFolder;
                    }
                }
                catch
                {
                    error = LaunchErrorCode.UnspecifiedError;
                }
                WindowManager.ShowMessage(Resx.LaunchGame, string.Format(Resx.UnableToLaunchGame, (int)error, error.ToString().ToUpper()));
            }

            if (CurrentRow.IsXex)
            {
                FileManager.Execute(CurrentRow.Path);
                if (ServerType != FtpServerType.FtpDll) ExecuteCloseCommand();
            }
        }

        #endregion

        #region ShowFtpLogCommand

        public DelegateCommand ShowFtpLogCommand { get; private set; }

        public void ExecuteShowFtpLogCommand()
        {
            EventAggregator.GetEvent<ShowFtpTraceWindowEvent>().Publish(new ShowFtpTraceWindowEventArgs(FileManager.TraceListener, Connection.Name));
        }

        #endregion

        #region ShutdownCommand

        public DelegateCommand ShutdownCommand { get; private set; }

        public bool CanExecuteShutdownCommand()
        {
            return FileManager.IsFSD;
        }

        public void ExecuteShutdownCommand()
        {
            if (WindowManager.Confirm(Resx.Shutdown, string.Format(Resx.ShutdownConfirmation, Connection.Name))) Shutdown();
        }

        #endregion

        #region StartContentScanCommand

        public DelegateCommand StartContentScanCommand { get; private set; }

        public bool CanExecuteStartContentScanCommand()
        {
            return FileManager.IsFSD;
        }

        public void ExecuteStartContentScanCommand()
        {
            try
            {
                HttpPostAsync("paths.html", string.Format("sessionid={0}&Action=Scan+All", HttpSessionId));
            }
            catch
            {
                WindowManager.ShowMessage(Resx.FsdContentScan, Resx.ContentScanFailedErrorMessage);
            }
        }

        #endregion

        #region OpenWebUICommand

        public DelegateCommand OpenWebUICommand { get; private set; }

        public bool CanExecuteOpenWebUICommand()
        {
            return FileManager.IsFSD;
        }

        public void ExecuteOpenWebUICommand()
        {
            Process.Start(string.Format("http://{0}", Connection.Address));
        }

        #endregion

        public FtpContentViewModel()
        {
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            CheckFreestyleDatabaseCommand = new DelegateCommand(ExecuteCheckFreestyleDatabaseCommand, CanExecuteCheckFreestyleDatabaseCommand);
            LaunchCommand = new DelegateCommand(ExecuteLaunchCommand, CanExecuteLaunchCommand);
            ShowFtpLogCommand = new DelegateCommand(ExecuteShowFtpLogCommand);
            ShutdownCommand = new DelegateCommand(ExecuteShutdownCommand, CanExecuteShutdownCommand);
            StartContentScanCommand = new DelegateCommand(ExecuteStartContentScanCommand, CanExecuteStartContentScanCommand);
            OpenWebUICommand = new DelegateCommand(ExecuteOpenWebUICommand, CanExecuteOpenWebUICommand);

            EventAggregator.GetEvent<TransferFinishedEvent>().Subscribe(OnTransferFinished);

            TitleRecognizer.RecognizeInaccessibleProfile = RecognizeInaccessibleProfile;
        }

        public override void LoadDataAsync(LoadCommand cmd, LoadDataAsyncParameters cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkHandler.Run(
                        () =>
                            {
                                Connection = (FtpConnectionItemViewModel)cmdParam.Payload;
                                return Connect();
                            },
                        result =>
                            {
                                IsLoaded = true;
                                ResumeCapability = result;
                                try
                                {
                                    ConnectCallback();
                                } 
                                catch(Exception ex)
                                {
                                    if (error != null) error.Invoke(this, new SomethingWentWrongException(Resx.IndetermineFtpConnectionError, ex));
                                    CloseCommand.Execute();
                                    return;
                                }
                                if (success != null) success.Invoke(this);
                            },
                        exception =>
                            {
                                if (error != null) error.Invoke(this, exception);
                            });
                    break;
                case LoadCommand.Restore:
                    var payload = cmdParam.Payload as BinaryContent;
                    if (payload == null) return;
                    WorkHandler.Run(
                        () =>
                            {
                                //TODO: upload binary
                                //File.WriteAllBytes(payload.TempFilePath, payload.Content);
                                //FileManager.RestoreConnection();
                                //FileManager.UploadFile(payload.FilePath, payload.TempFilePath);
                                return true;
                            },
                        result =>
                            {
                                if (success != null) success.Invoke(this);
                            },
                        exception =>
                            {
                                CloseCommand.Execute();
                                if (error != null) error.Invoke(this, exception);
                            });
                    break;
            }
        }

        private ResumeCapability Connect()
        {
            return FileManager.Connect(Connection.Model, true, OnGetListingDataReceived);
        }

        private void ConnectCallback()
        {
            _doContentScanOn.Clear();
            Initialize();
            SetFavorites();
            var defaultPath = string.IsNullOrEmpty(Connection.Model.DefaultPath)
                                  ? GetDefaultDrive()
                                  : Connection.Model.DefaultPath;

            var drive = GetDriveFromPath(defaultPath);
            if (drive != null && FileManager.FolderExists(defaultPath))
            {
                PathCache.Add(drive, defaultPath);
            }
            Drive = drive ?? Drives.First();

            if (!IsContentScanTriggerAvailable) return;

            //HACK: for testing purposes only
            if (FileManager.ServerType == FtpServerType.IIS)
            {
                ScanFolders = new Dictionary<int, FsdScanPath>()
                {
                    {1, new FsdScanPath
                    {
                        PathId = 1,
                        Path = "/Hdd1/Content/0000000000000000/",
                        ScanDepth = 2,
                        Drive = "Hdd1"
                    }}
                };
                return;
            }

            var credentials = GetHttpLogin();
            var username = credentials.UserName;
            var password = credentials.Password;

            switch (GetScanFolders(username, password))
            {
                case HttpStatusCode.OK:
                    if (!UserSettingsProvider.DisableFsdStatusPolling)
                    {
                        _statusUpdateTimer.Elapsed += StatusUpdateTimerOnElapsed;
                        _statusUpdateTimer.Start();
                    }
                    return;
                case HttpStatusCode.Unauthorized:
                    bool result;
                    do
                    {
                        var loginViewModel = Container.Resolve<ILoginViewModel>();
                        loginViewModel.Title = Resx.Login;
                        loginViewModel.Message = Resx.LoginToFreestyleDashHttpServer;
                        loginViewModel.Username = username;
                        loginViewModel.Password = password;
                        loginViewModel.IsRememberPasswordEnabled = true;

                        var login = WindowManager.ShowLoginDialog(loginViewModel);
                        if (login == null)
                        {
                            var answer = (DisableOption)WindowManager.ShowListInputDialog(Resx.DisableFsdContentScanTriggerTitle, Resx.DisableFsdContentScanTriggerMessage, DisableOption.None, GetDisableOptionList());
                            switch (answer)
                            {
                                case DisableOption.All:
                                    UserSettingsProvider.FsdContentScanTrigger = FsdContentScanTrigger.Disabled;
                                    break;
                                case DisableOption.Single:
                                    Connection.IsHttpAccessDisabled = true;
                                    break;
                            }
                            result = true;
                        }
                        else
                        {
                            username = login.Username;
                            password = login.Password;
                            var status = GetScanFolders(username, password);
                            if (status != HttpStatusCode.OK && status != HttpStatusCode.Unauthorized)
                            {
                                //TODO: handle different result then the previous one
                                result = true;
                            }
                            else
                            {
                                result = status != HttpStatusCode.Unauthorized;
                                if (result && login.RememberPassword == true)
                                {
                                    Connection.HttpUsername = username;
                                    Connection.HttpPassword = password;
                                    EventAggregator.GetEvent<ConnectionDetailsChangedEvent>().Publish(new ConnectionDetailsChangedEventArgs(Connection));
                                }
                            }
                        }
                    } while (!result);
                    break;
                case HttpStatusCode.RequestTimeout:
                    {
                        var answer = (DisableOption)WindowManager.ShowListInputDialog(Resx.DisableFsdContentScanTriggerTitle, Resx.DisableFsdContentScanTriggerMessage, DisableOption.None, GetDisableOptionList());
                        switch (answer)
                        {
                            case DisableOption.All:
                                UserSettingsProvider.FsdContentScanTrigger = FsdContentScanTrigger.Disabled;
                                break;
                            case DisableOption.Single:
                                Connection.IsHttpAccessDisabled = true;
                                break;
                        }
                    }
                    break;
            }
        }

        private NetworkCredential GetHttpLogin()
        {
            var username = Connection.HttpUsername;
            var password = Connection.HttpPassword;
            if (username == null)
            {
                switch (FileManager.ServerType)
                {
                    case FtpServerType.F3:
                        username = password = "f3http";
                        break;
                    case FtpServerType.FSD:
                        username = password = "fsdhttp";
                        break;
                    case FtpServerType.Aurora:
                    case FtpServerType.AuroraAlpha:
                        username = password = "xboxhttp";
                        break;
                    default:
                        throw new NotSupportedException("Invalid server type: " + FileManager.ServerType);
                }
            }
            return new NetworkCredential(username, password);
        }

        private string GetDefaultDrive()
        {
            switch (ServerType)
            {
                case FtpServerType.PlayStation3:
                    return "/dev_hdd0/";
                case FtpServerType.FtpDll:
                    return "/fHdd/";
                case FtpServerType.DashLaunch:
                    return "/Hdd/";
                default:
                    return "/Hdd1/";
            }
        }

        protected override FileSystemItemViewModel GetDriveFromPath(string path)
        {
            var r = new Regex("^/[A-Z0-9_-]+/", RegexOptions.IgnoreCase);
            var m = r.Match(path);
            return m.Success ? Drives.SingleOrDefault(d => d.Path == m.Value) : null;
        }

        private List<InputDialogOptionViewModel> GetDisableOptionList()
        {
            return Enum.GetValues(typeof (DisableOption))
                       .Cast<DisableOption>()
                       .Select(o => new InputDialogOptionViewModel
                       {
                           Value = o,
                           DisplayName = Resx.ResourceManager.EnumToTranslation(o)
                       })
                       .ToList();
        }

        public void RestoreConnection()
        {
            FileManager.RestoreConnection();
            NotifyPropertyChanged(ISCONNECTED);
            NotifyPropertyChanged(ISCONTENTSCANTRIGGERAVAILABLE);
            NotifyPropertyChanged(ISFSD);
            NotifyPropertyChanged(SERVERTYPE);
            NotifyPropertyChanged(ISVERIFICATIONENABLED);
            NotifyPropertyChanged(ISSHUTDOWNSUPPORTED);
            NotifyPropertyChanged(ISLAUNCHSUPPORTED);
        }

        public void Shutdown()
        {
            FileManager.Shutdown();
            CloseCommand.Execute();
        }

        private void OnTransferFinished(TransferFinishedEventArgs e)
        {
            if (e.Target != this || !IsContentScanTriggerAvailable) return;

            var scanFolder = GetCorrespondingScanFolder(CurrentFolder.Path);
            if (scanFolder == null) return;

            switch (UserSettingsProvider.FsdContentScanTrigger)
            {
                case FsdContentScanTrigger.AfterUpload:
                    TriggerContentScan(scanFolder.PathId);
                    break;
                case FsdContentScanTrigger.AfterConnectionClose:
                    _doContentScanOn.Add(scanFolder.PathId);
                    break;
            }
        }

        private HttpStatusCode GetScanFolders(string username, string password)
        {
            try
            {
                using (var client = new WebClient())
                {
                    byte[] response;
                    var url = string.Format("http://{0}/paths.html", Connection.Address);
                    if (username == string.Empty)
                    {
                        response = client.DownloadData(url);
                    }
                    else
                    {
                        byte[] body;
                        using (var ms = new MemoryStream())
                        {
                            var sw = new StreamWriter(ms);
                            sw.Write("j_username={0}&j_password={1}&Action=Login", username, password);
                            sw.Flush();
                            body = ms.ToArray();
                        }
                        client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        response = client.UploadData(url, body);
                    }
                    var responseString = Encoding.UTF8.GetString(response);
                    if (responseString.Contains("j_password") || responseString.Contains("Username / Password incorrect.  Please try again")) return HttpStatusCode.Unauthorized;

                    var r = new Regex(@"<tr.*?pathid:'(?<PathId>\d+)'.*?depth"">(?<ScanDepth>\d+).*?path"">(?<Path>.*?)</td>", RegexOptions.Singleline);
                    ScanFolders = new Dictionary<int, FsdScanPath>();
                    foreach (Match m in r.Matches(responseString))
                    {
                        try
                        {
                            var path = "/" + m.Groups["Path"].Value.Replace(":\\", "\\").Replace("\\", "/");
                            if (ScanFolders.Any(kvp => kvp.Value.Path == path)) continue;
                            var pathid = Int32.Parse(m.Groups["PathId"].Value);
                            var depth = Int32.Parse(m.Groups["ScanDepth"].Value);
                            var drive = m.Groups["Path"].Value.SubstringBefore(":");
                            ScanFolders.Add(pathid, new FsdScanPath
                            {
                                PathId = pathid,
                                Path = path,
                                ScanDepth = depth,
                                Drive = drive
                            });
                        }
                        catch
                        {
                        }
                    }

                    switch (ServerType)
                    {
                        case FtpServerType.F3:
                            const string sessionCookie = "session=";
                            var setCookie = client.ResponseHeaders[HttpResponseHeader.SetCookie].Split('&');
                            var session = setCookie.FirstOrDefault(c => c.StartsWith(sessionCookie));
                            if (session != null) HttpSessionId = session.Substring(sessionCookie.Length);
                            break;
                        case FtpServerType.FSD:
                            var sessionidRegex = new Regex(@"\{sessionid:'(.*?)'\}");
                            var m = sessionidRegex.Match(responseString);
                            if (m.Success) HttpSessionId = m.Groups[1].Value;
                            break;
                        default:
                            throw new NotSupportedException("Invalid server type: " + ServerType);
                    }
                    SetStatus(responseString);
                    return HttpStatusCode.OK;
                }
            }
            catch
            {
                return HttpStatusCode.RequestTimeout;
            }
        }

        private void StatusUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (UserSettingsProvider.DisableFsdStatusPolling)
            {
                _statusUpdateTimer.Stop();
                _statusUpdateTimer.Elapsed -= StatusUpdateTimerOnElapsed;
                return;
            }

            try
            {
                var response = HttpGetString(string.Empty, Encoding.UTF8);
                if (response == null)
                {
                    _statusUpdateTimer.Stop();
                }
                else
                {
                    _statusUpdateTimer.Interval = SetStatus(response) ? 500 : 3000;    
                }
            }
            catch
            {
                //intentional swallow
            }
        }

        private bool SetStatus(string responseString)
        {
            if (responseString == null) return false;
            var r = new Regex("<b>Status :</b>(.*?)</td>");
            var m = r.Match(responseString);
            _fsdStatus = null;
            if (m.Success)
            {
                var r2 = new Regex("( \\| )?FTP : Connected( \\| )?");
                _fsdStatus = r2.Replace(m.Groups[1].Value.Trim(), string.Empty);
            }
            UIThread.Run(() => NotifyPropertyChanged(SIZEINFO));
            return !string.IsNullOrWhiteSpace(_fsdStatus);
        }

        protected override string GetSizeInfo()
        {
            var sizeInfo = base.GetSizeInfo();
            if (!string.IsNullOrWhiteSpace(_fsdStatus))
            {
                sizeInfo += " | " + _fsdStatus;
            }
            return sizeInfo;
        }

        private FsdScanPath GetCorrespondingScanFolder(string path)
        {
            if (ScanFolders == null) return null;

            foreach (var scanPath in ScanFolders.Where(s => path.StartsWith(s.Value.Path)).Select(s => s.Value))
            {
                var relativePath = path.Replace(scanPath.Path, String.Empty).Trim('/');
                var depth = relativePath.Split('/').Length;
                if (scanPath.ScanDepth >= depth) return scanPath;
            }
            return null;
        }

        private void TriggerContentScan(int pathid)
        {
            try
            {
                HttpPostAsync("paths.html", string.Format("sessionid={0}&pathid={1}&Action=scan", HttpSessionId, pathid));
            }
            catch
            {
                WindowManager.ShowMessage(Resx.FsdContentScan, Resx.ContentScanFailedErrorMessage);
            }
        }

        public byte[] HttpGet(string target)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    client.Headers[HttpRequestHeader.Cookie] = "session=" + HttpSessionId;
                    client.Headers[HttpRequestHeader.Referer] = "http://" + Connection.Address;
                    var result = client.DownloadData(string.Format("http://{0}/{1}", Connection.Address, target));
                    return result;
                }
            }
            catch
            {
                //TODO: log exception
                return null;
            }
        }

        public string HttpGetString(string target, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.Default;
            var html = HttpGet(target);
            return html != null ? encoding.GetString(html) : null;
        }

        private byte[] HttpPost(string target, string formData)
        {
            using (var client = new WebClient())
            {
                byte[] body;
                using (var ms = new MemoryStream())
                {
                    var sw = new StreamWriter(ms);
                    sw.Write(formData);
                    sw.Flush();
                    body = ms.ToArray();
                }
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                client.Headers[HttpRequestHeader.Cookie] = "session=" + HttpSessionId;
                return client.UploadData(new Uri(string.Format("http://{0}/{1}", Connection.Address, target)), body);
            }
        }

        private void HttpPostAsync(string target, string formData)
        {
            using (var client = new WebClient())
            {
                byte[] body;
                using (var ms = new MemoryStream())
                {
                    var sw = new StreamWriter(ms);
                    sw.Write(formData);
                    sw.Flush();
                    body = ms.ToArray();
                }
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                client.Headers[HttpRequestHeader.Cookie] = "session=" + HttpSessionId;
                client.UploadDataAsync(new Uri(string.Format("http://{0}/{1}", Connection.Address, target)), body);
            }
        }

        public string HttpPostString(string target, string formData, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.Default;
            var html = HttpPost(target, formData);
            return html != null ? encoding.GetString(html) : null;
        }

        protected override void ChangeDrive()
        {
            //FtpTrace.WriteLine("[ChangeDrive]");
            if (!_driveLabelCache.ContainsKey(Drive.Path))
            {
                var path = String.Format("{0}name.txt", Drive.Path);
                string label = null;
                try
                {
                    var bytes = FileManager.ReadFileContent(path);
                    var name = Encoding.BigEndianUnicode.GetString(bytes);
                    name = name.SubstringBefore(Environment.NewLine) ?? name;
                    if (name.Length > 30) name = name.Substring(0, 30) + "...";
                    label = String.Format("[{0}]", name);
                }
                catch
                {
                }
                _driveLabelCache.Add(Drive.Path, label);
            }
            DriveLabel = _driveLabelCache[Drive.Path];
            base.ChangeDrive();
        }

        public override IList<FileSystemItem> GetList(string selectedPath)
        {
            return FileManager.ServerType == FtpServerType.PlayStation3
                ? FileManager.GetList(selectedPath)
                : base.GetList(selectedPath);
        }

        public override void Refresh(bool refreshCache, Action callback)
        {
            FileManager.ClearListingCache();
            base.Refresh(refreshCache, callback);
        }

        protected override void ChangeDirectoryCallback(IList<FileSystemItem> result, bool refreshCache)
        {
            base.ChangeDirectoryCallback(result, refreshCache);
            //FtpTrace.WriteLine("[FTP.ChangeDirectoryCallback] START");
            switch (ServerType)
            {
                case FtpServerType.PlayStation3:
                    //PS3 Transfer complete response string
                    var r = new Regex(string.Format(@"226 Transfer complete \[{0}\] \[ ([0-9]+.*?free) \]", Drive.Path.TrimEnd('/')));
                    var m = r.Match(FileManager.Log.ElementAt(1));
                    if (m.Success)
                    {
                        //TODO: localize
                        FreeSpaceText = m.Groups[1].Value;
                    }
                    break;
                case FtpServerType.Aurora:
                    var storageInfo = FileManager.StorageInfo();
                    if (storageInfo.ContainsKey(Drive.Name)) FreeSpaceText = storageInfo[Drive.Name].Replace("free", Resx.Free);
                    break;
            }
            //FtpTrace.WriteLine("[FTP.ChangeDirectoryCallback] END");
        }

        public override string GetTargetPath(string path)
        {
            return String.Format("{0}{1}", CurrentFolder.Path, path.Replace('\\', '/'));
        }

        protected override void AsyncErrorCallback(Exception ex)
        {
            base.AsyncErrorCallback(WrapFtpException(ex));
        }

        public override TransferResult CreateFolder(string path)
        {
            try
            {
                ValidateXtafPath(path);
                return base.CreateFolder(path);
            }
            catch (Exception ex)
            {
                throw WrapFtpException(ex);
            }
        }

        public override TransferResult Delete(FileSystemItem item)
        {
            try
            {
                return base.Delete(item);
            }
            catch (Exception ex)
            {
                throw WrapFtpException(ex);
            }
        }

        public override Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_fsdStatus))
                {
                    //TODO
                }
                ValidateXtafPath(path);
                return base.GetStream(path, mode, access, startPosition);
            }
            catch (Exception ex)
            {
                throw WrapFtpException(ex);
            }
        }

        public override bool CopyStream(FileSystemItem item, Stream stream, long startPosition = 0, long? byteLimit = null)
        {
            try
            {
                return base.CopyStream(item, stream, startPosition, byteLimit);
            }
            catch (Exception ex)
            {
                throw WrapFtpException(ex);
            }
        }

        protected override FileSystemItem RenameItemName(FileSystemItem item, string name)
        {
            ValidateXtafPath(name);
            return base.RenameItemName(item, name);
        }

        private void ValidateXtafPath(string path)
        {
            //Maximum Path Length
            if (path.Length > 240) throw new TransferException(TransferErrorType.PathIsTooLong, Resx.PathIsTooLong);

            //Maximum Filename Length
            var i = path.LastIndexOf(FileManager.Slash);
            if (path.Substring(i + 1).Length > 42) throw new TransferException(TransferErrorType.NameIsTooLong, Resx.NameIsTooLong);

            //TODO: [determine] if (FileManager.IsUTF8Supported) return;

            //Invalid character check
            if (new Regex(Xtaf.InvalidFileNameChars).IsMatch(path.Replace(new string(FileManager.Slash, 1), string.Empty)))
                throw new TransferException(TransferErrorType.NotSupporterCharactersInPath, Resx.SpecialCharactersNotSupported);
        }

        private Exception WrapFtpException(Exception exception)
        {
            if (FileManager.IsConnected) return exception;
            return new TransferException(TransferErrorType.LostConnection, string.Format(Resx.ConnectionLostMessage, Connection.Name), exception)
                       {
                           Pane = this
                       };
        }

        public TransferResult Verify(string remotePath, string localPath)
        {
            if (!FileManager.IsFSD) return TransferResult.Skipped;

            var match = FileManager.Verify(remotePath, localPath);
            if (!match) throw new FtpHashVerificationException(Resx.FtpHashVerificationFailed);
            return TransferResult.Ok;
        }

        public override Queue<QueueItem> PopulateQueue(FileOperation action, IEnumerable<FileSystemItem> selection)
        {
            var queue = base.PopulateQueue(action, selection);
            if (action == FileOperation.Delete) CheckQueueForGameDeletion(queue);
            return queue;
        }

        protected override bool ValidateDirectory(FileSystemItem item, FileOperation action)
        {
            if (item.Type == ItemType.Directory && 
                new Regex(string.Format(ProfileFilePattern, item.Name)).IsMatch(item.Path))
            {
                if (action == FileOperation.Copy)
                    UIThread.Run(() => EventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(new NotifyUserMessageEventArgs("LockedProfileCopyMessage", MessageIcon.Warning)));
                return false;
            }
            return base.ValidateDirectory(item, action);
        }

        //private bool GetTitleDataFromFreestyle(FileSystemItem item)
        //{
        //    if (Connection.IsHttpAccessDisabled) return false;

        //    var result = false;
        //    try
        //    {
        //        var rows = _contentItemsCache ?? GetContentItems();
        //        var title = rows.FirstOrDefault(r =>
        //        {
        //            var titleIdInt = Int32.Parse(r[FsdContentItemProperty.TitleId]);
        //            if (titleIdInt == 0) return false;
        //            var titleId = titleIdInt.ToString("X");
        //            return titleId == item.Name;
        //        });
        //        if (title != null)
        //        {
        //            var contentId = Int32.Parse(title[FsdContentItemProperty.Id]);
        //            item.Title = title[FsdContentItemProperty.Name];
        //            item.RecognitionState = RecognitionState.PartiallyRecognized;
        //            result = true;
        //            item.Thumbnail = HttpGet(string.Format("assets/gameicon.png?contentid={0:X2}", contentId));
        //            item.RecognitionState = RecognitionState.Recognized;
        //        }
        //        _contentItemsCache = rows;
        //    }
        //    catch
        //    {
        //    }
        //    return result;
        //}

        public List<FsdContentItemCells> GetContentItems()
        {
            string htmlString;
            var html = new HtmlDocument();
            IEnumerable<HtmlNode> rows = null;
            switch (ServerType)
            {
                case FtpServerType.F3:
                    htmlString = HttpGetString("gettable.html?name=ContentItems", Encoding.UTF8);
                    if (htmlString != null)
                    {
                        html.LoadHtml(htmlString);
                        rows = html.DocumentNode.QuerySelectorAll("table.GameContentHeader > tr").Skip(2);
                    }
                    break;
                case FtpServerType.FSD:
                    htmlString = HttpPostString("gettable?name=ContentItems", "sessionid=" + HttpSessionId, Encoding.UTF8);
                    if (htmlString != null)
                    {
                        html.LoadHtml(htmlString);
                        rows = html.DocumentNode.QuerySelectorAll("tr").Skip(1);
                    }
                    break;
                default:
                    throw new NotSupportedException("Invalid server type: " + ServerType);
            }
            return rows != null
                ? rows.Select(row => new FsdContentItemCells(row.SelectNodes("td"))).ToList()
                : new List<FsdContentItemCells>();
        }

        private bool RecognizeInaccessibleProfile(FileSystemItem item)
        {
            if (item.TitleType == TitleType.Profile && 
                item.LockMessage == Resx.ProfileIsInUseErrorMessage && 
                FileManager.HasWebUI)
            {
                try
                {
                    var credentials = GetHttpLogin();
                    var webClient = new WebClient {Credentials = credentials};
                    webClient.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials.UserName + ":" + credentials.Password)));
                    var json = webClient.DownloadString(string.Format("http://{0}:9999/getProfileInfo", Connection.Address));
                    if (!string.IsNullOrEmpty(json))
                    {
                        var profiles = json.FromJson<F3PluginGetProfileInfo[]>();
                        var profile = profiles.FirstOrDefault(p => p.SignedIn == 1 && p.Xuid == item.Name);
                        if (profile != null)
                        {
                            item.Title = profile.Gamertag;
                            item.RecognitionState = RecognitionState.PartiallyRecognized;
                            var bytes = webClient.DownloadData(string.Format("http://{0}:9999/getProfileImage?uuid={1}", Connection.Address, profile.Index));
                            if (bytes != null)
                            {
                                item.Thumbnail = bytes;
                                item.RecognitionState = RecognitionState.Recognized;
                            }
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                }
            }
            return false;
        }

        private void CheckQueueForGameDeletion(Queue<QueueItem> queue)
        {
            if (IsFSD && queue.Any(item => item.FileSystemItem.TitleType == TitleType.Game && GetCorrespondingScanFolder(item.FileSystemItem.Path) != null))
            {
                UIThread.Run(() => EventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(new NotifyUserMessageEventArgs("GameDeletionMessage", MessageIcon.Warning)));
            }
        }

        private void OnGetListingDataReceived(object sender, FtpGetListingEventArgs e)
        {
            if (e.Size < 20480) return;
            UIThread.Run(() => ProgressMessage = string.Format(Resx.ChangingDirectoryDataReceived, e.Size));
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            if (CheckFreestyleDatabaseCommand == null) return;
            CheckFreestyleDatabaseCommand.RaiseCanExecuteChanged();
            LaunchCommand.RaiseCanExecuteChanged();
            ShowFtpLogCommand.RaiseCanExecuteChanged();
            ShutdownCommand.RaiseCanExecuteChanged();
        }

        public override void Dispose()
        {
            EventAggregator.GetEvent<CloseFtpTraceWindowEvent>().Publish(new CloseFtpTraceWindowEventArgs(FileManager.TraceListener, false));
            FileManager.Disconnect(OnGetListingDataReceived);
            _statusUpdateTimer.Stop();
            _statusUpdateTimer.Elapsed -= StatusUpdateTimerOnElapsed;
            _doContentScanOn.ForEach(TriggerContentScan);
            if (CurrentFolder != null) Connection.Model.DefaultPath = CurrentFolder.Path;
            base.Dispose();
        }

        public override object Close(object data)
        {
            Dispose();
            return Connection;
        }
    }
}