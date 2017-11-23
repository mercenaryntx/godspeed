using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Shell;
using Neurotoxin.Godspeed.Core.Diagnostics;
using Neurotoxin.Godspeed.Core.Exceptions;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using System.Linq;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{

    public class TransferManagerViewModel : CommonViewModelBase, ITransferManagerViewModel
    {
        private const string RenameFromPattern = @"([\/]){0}$";
        private const string RenameToPattern = @"${{1}}{0}";
        private readonly Ticker _elapsedTimeMeter = new Ticker { Interval = 100 };
        private Queue<QueueItem> _queue;
        private RemoteCopyMode _remoteCopy;
        private CopyAction _rememberedCopyAction;
        private TransferErrorDialogResult _skipAll;
        private long _currentFileBytesTransferred;
        private bool _isAborted;
        private bool _isContinued;
        private bool _sourceChanged;
        private bool _targetChanged;
        private bool _deleteAll;
        private Shutdown _shutdown;

        private readonly IStatisticsViewModel _statistics;
        private readonly IUserSettingsProvider _userSettingsProvider;

        #region Properties

        public IFileListPaneViewModel SourcePane { get; private set; }
        public IFileListPaneViewModel TargetPane { get; private set; }

        public string ShutdownXbox
        {
            get
            {
                var ftp = Pane<FtpContentViewModel>();
                return string.Format(Resx.ShutdownXbox, ftp != null ? ftp.Connection.Name : "Xbox");
            }
        }

        private const string USERACTION = "UserAction";
        private FileOperation _userAction;
        public FileOperation UserAction
        {
            get { return _userAction; }
            set { _userAction = value; NotifyPropertyChanged(USERACTION); }
        }

        private const string TRANSFERACTION = "TransferAction";
        private string _transferAction;
        public string TransferAction
        {
            get { return _transferAction; }
            set { _transferAction = value; NotifyPropertyChanged(TRANSFERACTION); }
        }

        private const string SOURCEFILE = "SourceFile";
        private string _sourceFile;
        public string SourceFile
        {
            get { return _sourceFile; }
            set { _sourceFile = value; NotifyPropertyChanged(SOURCEFILE); }
        }

        private const string CURRENTFILEPROGRESS = "CurrentFileProgress";
        private int _currentFileProgress;
        public int CurrentFileProgress
        {
            get { return _currentFileProgress; }
            set { _currentFileProgress = value; NotifyPropertyChanged(CURRENTFILEPROGRESS); }
        }

        private const string TOTALPROGRESS = "TotalProgress";
        public int TotalProgress
        {
            get
            {
                switch (UserAction)
                {
                    case FileOperation.Delete:
                        return FilesTransferred*100/FileCount;
                    default:
                        return TotalBytes != 0 ? (int)(BytesTransferred * 100 / TotalBytes) : 0;
                }
            }
        }

        private const string TOTALPROGRESSDOUBLE = "TotalProgressDouble";
        public double TotalProgressDouble
        {
            get { return (double) TotalProgress/100; }
        }

        private const string FILESTRANSFERRED = "FilesTransferred";
        private int _filesTransferred;
        public int FilesTransferred
        {
            get { return _filesTransferred; }
            set
            {
                _filesTransferred = value; 
                NotifyPropertyChanged(FILESTRANSFERRED); 
                NotifyPropertyChanged(TOTALPROGRESS);
                NotifyPropertyChanged(TOTALPROGRESSDOUBLE);
            }
        }

        private const string FILECOUNT = "FileCount";
        private int _fileCount;
        public int FileCount
        {
            get { return _fileCount; }
            set { _fileCount = value; NotifyPropertyChanged(FILECOUNT); }
        }

        private const string BYTESTRANSFERRED = "BytesTransferred";
        private long _bytesTransferred;
        public long BytesTransferred
        {
            get { return _bytesTransferred; }
            set
            {
                _bytesTransferred = value;
                NotifyPropertyChanged(BYTESTRANSFERRED);
                NotifyPropertyChanged(TOTALPROGRESS);
                NotifyPropertyChanged(TOTALPROGRESSDOUBLE);
            }
        }

        private const string TOTALBYTES = "TotalBytes";
        private long _totalBytes;
        public long TotalBytes
        {
            get { return _totalBytes; }
            set { _totalBytes = value; NotifyPropertyChanged(TOTALBYTES); }
        }

        private const string SPEED = "Speed";
        private int _speed;
        public int Speed
        {
            get { return _speed; }
            set { _speed = value; NotifyPropertyChanged(SPEED); }
        }

        private const string ELAPSEDTIME = "ElapsedTime";
        private TimeSpan _elapsedTime;
        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set { _elapsedTime = value; NotifyPropertyChanged(ELAPSEDTIME); }
        }

        private const string REMAININGTIME = "RemainingTime";
        private TimeSpan _remainingTime;
        public TimeSpan RemainingTime
        {
            get { return _remainingTime; }
            set { _remainingTime = value; NotifyPropertyChanged(REMAININGTIME); }
        }

        private const string PROGRESSSTATE = "ProgressState";
        private TaskbarItemProgressState _progressState;
        public TaskbarItemProgressState ProgressState
        {
            get { return _progressState; }
            set { _progressState = value; NotifyPropertyChanged(PROGRESSSTATE); }
        }

        private const string ISPAUSED = "IsPaused";
        private bool _isPaused;
        public bool IsPaused
        {
            get { return _isPaused; }
            set { _isPaused = value; NotifyPropertyChanged(ISPAUSED); }
        }

        public bool IsVerificationSupported
        {
            get
            {
                var ftp = Pane<FtpContentViewModel>();
                return ftp != null && ftp.IsFSD;
            }
        }

        public bool IsShutdownSupported
        {
            get
            {
                var ftp = Pane<FtpContentViewModel>();
                return ftp != null && ftp.IsShutdownSupported;
            }
        }

        public bool IsResumeSupported
        {
            get
            {
                return SourcePane.ResumeCapability.HasFlag(ResumeCapability.Restart) &&
                       (TargetPane == null || TargetPane.ResumeCapability.HasFlag(ResumeCapability.Append));
            }
        }

        private const string ISVERIFICATIONENABLED = "IsVerificationEnabled";
        public bool IsVerificationEnabled
        {
            get { return _userSettingsProvider.VerifyFileHashAfterFtpUpload; }
            set { _userSettingsProvider.VerifyFileHashAfterFtpUpload = value; NotifyPropertyChanged(ISVERIFICATIONENABLED); }
        }

        private const string ISSHUTDOWNPCENABLED = "IsShutdownPcEnabled";
        public bool IsShutdownPcEnabled
        {
            get { return _shutdown.HasFlag(Shutdown.PC); }
            set { _shutdown = EnumHelper.SetFlag(_shutdown, Shutdown.PC, value); NotifyPropertyChanged(ISSHUTDOWNPCENABLED); }
        }

        private const string ISSHUTDOWNXBOXENABLED = "IsShutdownXboxEnabled";
        public bool IsShutdownXboxEnabled
        {
            get { return _shutdown.HasFlag(Shutdown.Xbox); }
            set { _shutdown = EnumHelper.SetFlag(_shutdown, Shutdown.Xbox, value); NotifyPropertyChanged(ISSHUTDOWNXBOXENABLED); }
        }

        #endregion

        #region PauseCommand

        public DelegateCommand PauseCommand { get; private set; }

        private void ExecutePauseCommand()
        {
            IsPaused = true;
            SourcePane.Abort();
            if (TargetPane != null) TargetPane.Abort();
        }

        #endregion

        #region ContinueCommand

        public DelegateCommand ContinueCommand { get; private set; }

        private void ExecuteContinueCommand()
        {
            _elapsedTimeMeter.Start();
            ProgressState = TaskbarItemProgressState.Normal;
            IsPaused = false;
            _isContinued = true;
            ProcessQueueItem(CopyAction.Resume);
        }

        #endregion

        public TransferManagerViewModel(IUserSettingsProvider userSettingsProvider, IStatisticsViewModel statistics)
        {
            _userSettingsProvider = userSettingsProvider;
            _statistics = statistics;
            _elapsedTimeMeter.Tick += OnTransferProgressTick;
            PauseCommand = new DelegateCommand(ExecutePauseCommand);
            ContinueCommand = new DelegateCommand(ExecuteContinueCommand);
            EventAggregator.GetEvent<TransferProgressChangedEvent>().Subscribe(OnTransferProgressChanged);
            EventAggregator.GetEvent<ShowCorrespondingErrorEvent>().Subscribe(OnShowCorrespondingError);
        }

        public void Copy(IFileListPaneViewModel sourcePane, IFileListPaneViewModel targetPane, IEnumerable<FileSystemItem> queue)
        {
            SourcePane = sourcePane;
            TargetPane = targetPane;
            AsyncJob(() => SourcePane.PopulateQueue(FileOperation.Copy, queue), 
                      q => InitializeTransfer(q, FileOperation.Copy),
                      PopulationError);
        }

        public void Move(IFileListPaneViewModel sourcePane, IFileListPaneViewModel targetPane, IEnumerable<FileSystemItem> queue)
        {
            SourcePane = sourcePane;
            TargetPane = targetPane;
            AsyncJob(() => SourcePane.PopulateQueue(FileOperation.Copy, queue), 
                      q => InitializeTransfer(q, FileOperation.Move), 
                      PopulationError);
        }

        public void Delete(IFileListPaneViewModel sourcePane, IEnumerable<FileSystemItem> queue)
        {
            SourcePane = sourcePane;
            TargetPane = null;
            AsyncJob(() => SourcePane.PopulateQueue(FileOperation.Delete, queue), 
                      q => InitializeTransfer(q, FileOperation.Delete),
                      PopulationError);    
        }

        private OperationResult ExecuteCopy(QueueItem queueitem, CopyAction? action, string rename)
        {
            var item = queueitem.FileSystemItem;
            var sourcePath = item.GetRelativePath(SourcePane.CurrentFolder.Path);
            var targetPath = TargetPane.GetTargetPath(sourcePath);
            TransferResult result;

            switch (item.Type)
            {
                case ItemType.Directory:
                case ItemType.Link:
                    if (TargetPane.CreateFolder(targetPath) == TransferResult.Ok)
                    {
                        var i = 1;
                        QueueItem qi;
                        while (i < _queue.Count && (qi = _queue.ElementAt(i)) != null && qi.FileSystemItem.Path.StartsWith(item.Path))
                        {
                            qi.CopyAction = CopyAction.Overwrite;
                            i++;
                        }
                    }
                    result = TransferResult.Ok;
                    break;
                case ItemType.File:
                    if (action == CopyAction.Rename && !string.IsNullOrEmpty(rename))
                    {
                        var r = new Regex(string.Format(RenameFromPattern, item.Name), RegexOptions.IgnoreCase);
                        targetPath = r.Replace(targetPath, string.Format(RenameToPattern, rename));
                        action = CopyAction.CreateNew;
                    }

                    var a = action ?? _rememberedCopyAction;

                    FileMode mode;
                    FileExistenceInfo exists;
                    long startPosition = 0;
                    switch (a)
                    {
                        case CopyAction.CreateNew:
                            //TODO: check what happens if targetPath contains spec.char
                            exists = TargetPane.FileExists(targetPath);
                            if (exists)
                                throw new TransferException(TransferErrorType.WriteAccessError, Resx.TargetAlreadyExists)
                                          {
                                              SourceFile = item.Path,
                                              TargetFile = targetPath,
                                              TargetFileSize = exists.Size
                                          };
                            mode = FileMode.CreateNew;
                            break;
                        case CopyAction.Overwrite:
                            mode = FileMode.Create;
                            break;
                        case CopyAction.OverwriteSmaller:
                            var target = TargetPane.FileExists(targetPath);
                            if (target.Exists && target.Size >= item.Size) return new OperationResult(TransferResult.Skipped, targetPath);
                            mode = FileMode.Create;
                            break;
                        case CopyAction.Resume:
                            mode = FileMode.Append;
                            exists = TargetPane.FileExists(targetPath);
                            if (exists) startPosition = exists.Size;
                            break;
                        default:
                            throw new ArgumentException("Invalid Copy action: " + action);
                    }

                    UIThread.Run(() => { TransferAction = GetCopyActionText(); });

                    switch (_remoteCopy)
                    {
                        case RemoteCopyMode.Disabled:
                            using (var targetStream = TargetPane.GetStream(targetPath, mode, FileAccess.Write, startPosition))
                            {
                                result = SourcePane.CopyStream(item, targetStream, startPosition) ? TransferResult.Ok : TransferResult.Aborted;
                            }
                            break;
                        case RemoteCopyMode.Download:
                            var sourceName = RemoteChangeDirectory(item.Path);
                            Telnet.Download(sourceName, targetPath, item.Size ?? 0, startPosition, (p, t, total, sp) => OnTransferProgressChanged(new TransferProgressChangedEventArgs(p, t, total, sp)));
                            result = TransferResult.Ok;
                            break;
                        case RemoteCopyMode.Upload:
                            var targetName = RemoteChangeDirectory(targetPath);
                            Telnet.Upload(targetName, item.Path, item.Size ?? 0, startPosition, (p, t, total, sp) => OnTransferProgressChanged(new TransferProgressChangedEventArgs(p, t, total, sp)));
                            result = TransferResult.Ok;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
            return new OperationResult(result, targetPath);
        }

        private OperationResult ExecuteDelete(QueueItem item)
        {
            return new OperationResult(SourcePane.Delete(item.FileSystemItem));
        }

        private OperationResult ExecuteVerification(QueueItem item)
        {
            UIThread.Run(() => TransferAction = Resx.Verifying);
            string remotePath;
            string localPath;
            var ftp = TargetPane as FtpContentViewModel;
            if (ftp == null)
            {
                ftp = SourcePane as FtpContentViewModel;
                if (ftp == null) throw new ApplicationException();

                localPath = (string) item.Payload;
                remotePath = item.FileSystemItem.Path;
            }
            else
            {
                remotePath = (string) item.Payload;
                localPath = item.FileSystemItem.Path;
            }
            var verificationResult = ftp.Verify(remotePath, localPath);
            return new OperationResult(verificationResult);
        }

        private void ProcessQueueItem(CopyAction? action = null, string rename = null)
        {
            if (_queue.Count > 0)
            {
                var queueitem = _queue.Peek();
                var item = queueitem.FileSystemItem;
                SourceFile = item.GetRelativePath(SourcePane.CurrentFolder.Path);

                if (queueitem.Confirmation)
                {
                    switch (queueitem.Operation)
                    {
                        case FileOperation.Delete:
                            if (!_deleteAll)
                            {
                                var payload = queueitem.Payload as FileSystemItem;
                                if (payload == null) throw new Exception("payload cannot be null");
                                _elapsedTimeMeter.Stop();
                                var result = WindowManager.ShowDeleteConfirmationDialog(payload.Path);
                                _elapsedTimeMeter.Start();
                                switch (result)
                                {
                                    case DeleteConfirmationResult.Delete:
                                        //do nothing
                                        break;
                                    case DeleteConfirmationResult.DeleteAll:
                                        _deleteAll = true;
                                        break;
                                    case DeleteConfirmationResult.Skip:
                                        while (queueitem.FileSystemItem != payload)
                                        {
                                            _queue.Dequeue();
                                            queueitem = _queue.Peek();
                                        }
                                        ProcessSuccess(new OperationResult(TransferResult.Skipped));
                                        return;
                                    case DeleteConfirmationResult.Cancel:
                                        FinishTransfer();
                                        return;
                                }
                            }
                            break;
                        default:
                            throw new NotSupportedException("Invalid operation: " + queueitem.Operation);
                    }
                }

                WorkHandler.Run(() =>
                {
                    switch (queueitem.Operation)
                    {
                        case FileOperation.Copy:
                            return ExecuteCopy(queueitem, action ?? queueitem.CopyAction, rename);
                        case FileOperation.Delete:
                            return ExecuteDelete(queueitem);
                        case FileOperation.Verify:
                            return ExecuteVerification(queueitem);
                        default:
                            throw new NotSupportedException("Invalid transfer type: " + queueitem.Operation);
                    }
                }, ProcessSuccess, ProcessError);
                return;
            }
            FinishTransfer();
        }

        private void ProcessSuccess(OperationResult result)
        {
            FileSystemItem deselect = null;
            var queueItem = _queue.Peek();
            switch (result.Result)
            {
                case TransferResult.Ok:
                    if (queueItem.Operation == FileOperation.Copy) _targetChanged = true;
                    if (queueItem.Operation == FileOperation.Copy && IsVerificationSupported && IsVerificationEnabled && queueItem.FileSystemItem.Type == ItemType.File)
                    {
                        queueItem.Operation = FileOperation.Verify;
                        queueItem.Payload = result.TargetPath;
                    } 
                    else if ((queueItem.Operation == FileOperation.Copy || queueItem.Operation == FileOperation.Verify) && UserAction == FileOperation.Move)
                    {
                        if (queueItem.FileSystemItem.Type == ItemType.Directory)
                        {
                            _queue.Dequeue();
                            _queue.Enqueue(new QueueItem(queueItem.FileSystemItem, FileOperation.Delete));
                        }
                        else
                        {
                            queueItem.Operation = FileOperation.Delete;
                        }
                    }
                    else
                    {
                        if (queueItem.Operation == FileOperation.Delete) _sourceChanged = true;
                        if (UserAction != FileOperation.Delete) _statistics.FilesTransferred++;
                        FilesTransferred++;
                        deselect = queueItem.FileSystemItem;
                        _queue.Dequeue();
                    }
                    break;
                case TransferResult.Skipped:
                    _queue.Dequeue();
                    break;
                case TransferResult.Aborted:
                    if (queueItem.Operation == FileOperation.Copy) _targetChanged = true;
                    if (IsPaused)
                    {
                        queueItem.CopyAction = CopyAction.Resume;
                    } 
                    else
                    {
                        _queue.Dequeue();
                    }
                    break;
            }

            _currentFileBytesTransferred = 0;

            if (deselect != null)
            {
                var vm = SourcePane.SelectedItems.FirstOrDefault(i => deselect.Path.StartsWith(i.Path));
                if (vm != null && (vm.Type == ItemType.File || !_queue.Any(q => q.FileSystemItem.Path.StartsWith(vm.Path))))
                {
                    vm.IsSelected = false;
                }
            }

            if (IsPaused)
            {
                PauseMeters();
            } 
            else
            {
                ProcessQueueItem();
            }
        }

        private void ProcessError(Exception exception)
        {
            if (_isAborted) { FinishTransfer(); return; }
            if (IsPaused) return;

            var ftp = Pane<FtpContentViewModel>();

            if (ftp != null && !ftp.IsConnected)
            {
                exception = new TransferException(TransferErrorType.LostConnection, string.Format(Resx.ConnectionLostMessage, ftp.Connection.Name), exception)
                                {
                                    Pane = ftp
                                };
            }

            var result = ShowCorrespondingErrorDialog(exception);
            switch (result.Behavior)
            {
                case ErrorResolutionBehavior.Retry:
                    BytesTransferred -= _currentFileBytesTransferred;
                    _currentFileBytesTransferred = 0;
                    ProcessQueueItem(result.Action);
                    break;
                case ErrorResolutionBehavior.Rename:
                    RenameExistingFile((TransferException)exception, CopyAction.Rename, ProcessQueueItem, ProcessError);
                    break;
                case ErrorResolutionBehavior.Skip:
                    if (_queue != null) BytesTransferred += _queue.Peek().FileSystemItem.Size ?? 0 - _currentFileBytesTransferred;
                    ProcessSuccess(new OperationResult(TransferResult.Skipped));
                    break;
                case ErrorResolutionBehavior.Cancel:
                    FinishTransfer();
                    break;
            }
        }

        private string GetCopyActionText()
        {
            if (TargetPane is FtpContentViewModel) return Resx.Upload;
            if (SourcePane is FtpContentViewModel) return Resx.Download;
            if (TargetPane is StfsPackageContentViewModel || TargetPane is CompressedFileContentViewModel) return Resx.Inject;
            if (SourcePane is StfsPackageContentViewModel || SourcePane is CompressedFileContentViewModel) return Resx.Extract;
            return Resx.ResourceManager.GetString(UserAction.ToString());
        }

        private void OnTransferProgressTick(object sender, ElapsedEventArgs e)
        {
            var elapsed = _elapsedTimeMeter.Elapsed;
            var transferred = BytesTransferred;
            ElapsedTime = elapsed;

            if (elapsed.Ticks <= 0 || transferred <= 0) return;
            var estimated = new TimeSpan((long)Math.Floor((double)elapsed.Ticks / transferred * TotalBytes));
            RemainingTime = estimated - elapsed;

            Speed = (int)Math.Floor(transferred / elapsed.TotalMilliseconds * 1000 / 1024);
        }

        private void OnTransferProgressChanged(TransferProgressChangedEventArgs args)
        {
            if (_isContinued)
            {
                _isContinued = false;
                return;
            }

            UIThread.Run(() =>
                {
                    if (args.Percentage == 100) _statistics.BytesTransferred += args.TotalBytesTransferred;
                    CurrentFileProgress = args.Percentage > 0 ? args.Percentage : 0;
                    _currentFileBytesTransferred += args.Transferred;
                    BytesTransferred += args.Transferred;
                });
        }

        private void OnShowCorrespondingError(ShowCorrespondingErrorEventArgs e)
        {
            ShowCorrespondingErrorDialog(e.Exception, e.FeedbackNeeded);
        }

        public void InitializeTransfer(Queue<QueueItem> queue, FileOperation mode)
        {
            _queue = queue;
            _paneCache.Clear();
            _isPaused = false;
            _isAborted = false;
            _isContinued = false;
            _deleteAll = false;
            _skipAll = null;
            UserAction = mode;
            TransferAction = mode == FileOperation.Copy ? GetCopyActionText() : Resx.ResourceManager.GetString(mode.ToString());
            _rememberedCopyAction = CopyAction.CreateNew;
            _currentFileBytesTransferred = 0;
            CurrentFileProgress = 0;
            FilesTransferred = 0;
            FileCount = _queue.Count;
            BytesTransferred = 0;
            TotalBytes = _queue.Where(item => item.FileSystemItem.Type == ItemType.File).Sum(item => item.FileSystemItem.Size ?? 0);
            Speed = 0;
            ElapsedTime = new TimeSpan(0);
            RemainingTime = new TimeSpan(0);

            WorkHandler.Run(BeforeTransferStart, BeforeTransferStartCallback);
        }

        private TransferInitializationResult BeforeTransferStart()
        {
            if (UserAction == FileOperation.Delete) return new TransferInitializationResult(RemoteCopyMode.Disabled);
            var result = new TransferInitializationResult
                             {
                                 TargetFreeSpace = TargetPane.FreeSpace
                             };
            if (result.TargetFreeSpace == null || result.TargetFreeSpace > TotalBytes)
            {
                var ftpPane = Pane<FtpContentViewModel>();
                var localPane = Pane<LocalFileSystemContentViewModel>();
                if (ftpPane != null && localPane != null && localPane.IsNetworkDrive && _userSettingsProvider.UseRemoteCopy)
                {
                    try
                    {
                        OpenTelnetSession(localPane, ftpPane);
                        result.RemoteCopyMode = SourcePane == ftpPane ? RemoteCopyMode.Download : RemoteCopyMode.Upload;
                    }
                    catch (TelnetException exception)
                    {
                        result.RemoteCopyMode = RemoteCopyMode.Disabled;
                        result.TelnetException = exception;
                    }
                }
            }
            return result;
        }

        private void BeforeTransferStartCallback(TransferInitializationResult result)
        {
            if (result.TargetFreeSpace.HasValue && result.TargetFreeSpace <= TotalBytes)
            {
                WindowManager.ShowMessage(Resx.IOError, Resx.NotEnoughFreeSpaceOnTargetDevice);
                return;
            }
            if (result.TelnetException != null)
            {
                var dialog = new RemoteCopyErrorDialog(result.TelnetException);
                if (dialog.ShowDialog() == false) return;
                if (dialog.TurnOffRemoteCopy) _userSettingsProvider.UseRemoteCopy = false;
            }
            _remoteCopy = result.RemoteCopyMode;
            ProgressState = TaskbarItemProgressState.Normal;
            EventAggregator.GetEvent<TransferStartedEvent>().Publish(new TransferStartedEventArgs(this));
            _elapsedTimeMeter.Restart();
            ProcessQueueItem();
        }

        internal TransferErrorDialogResult ShowCorrespondingErrorDialog(Exception exception, bool feedbackNeeded = true)
        {
            var transferException = exception as TransferException;
            var exceptionType = transferException != null ? transferException.Type : TransferErrorType.NotSpecified;

            _elapsedTimeMeter.Stop();
            
            var result = new TransferErrorDialogResult(ErrorResolutionBehavior.Cancel);
            switch (exceptionType)
            {
                case TransferErrorType.NotSpecified:
                    {
                        if (feedbackNeeded)
                        {
                            result = _skipAll ?? WindowManager.ShowIoErrorDialog(exception);
                        } 
                        else
                        {
                            WindowManager.ShowMessage(Resx.IOError, exception.Message);
                        }
                    }
                    break;
                case TransferErrorType.WriteAccessError:
                case TransferErrorType.NotSupporterCharactersInPath:
                case TransferErrorType.NameIsTooLong:
                case TransferErrorType.PathIsTooLong:
                    {
                        if (_skipAll != null)
                        {
                            result = _skipAll;
                        }
                        else
                        {
                            if (_queue == null)
                            {
                                WindowManager.ShowMessage(Resx.IOError, exception.Message);
                            }
                            else
                            {
                                var sourceFile = _queue.Peek().FileSystemItem;
                                var viewModel = WindowManager.GetWriteErrorDialogViewModel(transferException);
                                if (exceptionType == TransferErrorType.WriteAccessError)
                                {
                                    if (IsResumeSupported && sourceFile.Size > transferException.TargetFileSize)
                                    {
                                        viewModel.IsResumeEnabled = true;
                                        viewModel.IsResumeAllEnabled = true;
                                    }
                                    if (transferException.TargetFile != null) TargetPane.GetItemViewModel(transferException.TargetFile);
                                }
                                SourcePane.GetItemViewModel(sourceFile.Path);
                                result = WindowManager.ShowWriteErrorDialog(viewModel);
                            }
                        }
                    }
                    break;
                case TransferErrorType.LostConnection:
                    var ftp = (FtpContentViewModel)transferException.Pane;
                    if (WindowManager.ShowReconnectionDialog(exception) == true)
                    {
                        try
                        {
                            ftp.RestoreConnection();
                            ftp.Refresh(false);
                        } 
                        catch (Exception ex)
                        {
                            WindowManager.ShowMessage(Resx.ConnectionFailed, string.Format(Resx.CannotReestablishConnection, ex.Message));
                            ftp.CloseCommand.Execute();
                        }
                    }
                    else
                    {
                        ftp.CloseCommand.Execute();
                    }
                    break;
                default:
                    throw new NotSupportedException("Invalid transfer error type: " + exceptionType);
            }

            //TODO: refactor scoping
            if (result.Scope == CopyActionScope.All)
            {
                if (result.Action.HasValue) _rememberedCopyAction = result.Action.Value;
                if (result.Behavior == ErrorResolutionBehavior.Skip) _skipAll = result;
            }
            _elapsedTimeMeter.Start();
            return result;
        }

        private void OpenTelnetSession(LocalFileSystemContentViewModel local, FtpContentViewModel ftp)
        {
            var connection = ftp.Connection;
            Telnet.OpenSession(local.Drive.Path, local.Drive.FullPath, connection.Address, connection.Port ?? 21, connection.Username, connection.Password);
        }

        private void CloseTelnetSession()
        {
            Telnet.CloseSession();
        }

        private void PauseMeters()
        {
            _elapsedTimeMeter.Stop();
            ProgressState = TaskbarItemProgressState.Indeterminate;
        }

        public void AbortTransfer()
        {
            if (SourcePane == null) return;

            _isAborted = true;
            if (_remoteCopy == RemoteCopyMode.Disabled)
            {
                SourcePane.Abort();
                if (TargetPane != null) TargetPane.Abort();
            }
            lock (_queue)
            {
                var actualItem = _queue.Peek();
                _queue.Clear();
                _queue.Enqueue(actualItem);
            }
            if (_isPaused)
            {
                UIThread.BeginRun(FinishTransfer);
            }
        }

        private void FinishTransfer()
        {
            _queue = null;
            _statistics.TimeSpentWithTransfer += _elapsedTimeMeter.Elapsed;
            if (_remoteCopy != RemoteCopyMode.Disabled) CloseTelnetSession();
            _elapsedTimeMeter.Stop();
            ProgressState = TaskbarItemProgressState.None;

            var args = new TransferFinishedEventArgs(this, SourcePane, TargetPane);

            if (_shutdown != Shutdown.Disabled && (UserAction == FileOperation.Copy || UserAction == FileOperation.Move))
            {
                ShutdownDialogViewModel vm;
                if (_shutdown.HasFlag(Shutdown.Xbox))
                {
                    var ftp = Pane<FtpContentViewModel>();
                    if (ftp == SourcePane) _sourceChanged = false;
                    if (ftp == TargetPane) _targetChanged = false;
                    vm = new ShutdownDialogViewModel(WindowManager, _shutdown, ftp);
                } 
                else
                {
                    vm = new ShutdownDialogViewModel(WindowManager, _shutdown);
                }
                WindowManager.ShowModelessWindow<ShutdownDialog, ShutdownDialogViewModel>(vm);
            }

            if (_sourceChanged) SourcePane.Refresh(false);
            if (_targetChanged && TargetPane != null) TargetPane.Refresh(false);
            EventAggregator.GetEvent<TransferFinishedEvent>().Publish(args);

            SourcePane = null;
            TargetPane = null;
        }

        private void RenameExistingFile(TransferException exception, CopyAction? action, Action<CopyAction?, string> rename, Action<Exception> chooseDifferentOption)
        {
            var name = WindowManager.ShowTextInputDialog(Resx.Rename, Resx.NewName + Strings.Colon, Path.GetFileName(exception.TargetFile), null);
            if (!string.IsNullOrWhiteSpace(name))
            {
                rename.Invoke(action, name);
            }
            else
            {
                chooseDifferentOption.Invoke(exception);
            }
        }

        private void AsyncJob<T>(Func<T> work, Action<T> success, Action<Exception> error = null)
        {
            EventAggregator.GetEvent<AsyncJobStartedEvent>().Publish(new AsyncJobStartedEventArgs(this));
            var finished = false;
            WorkHandler.Run(
                () =>
                {
                    Thread.Sleep(3000);
                    return true;
                },
                b =>
                {
                    if (finished) return;
                    WindowManager.ShowMessage(Resx.ApplicationIsBusy, Resx.PleaseWait, NotificationMessageFlags.NonClosable);
                });
            WorkHandler.Run(work,
                b =>
                {
                    WindowManager.CloseMessage();
                    finished = true;
                    success.Invoke(b);
                    EventAggregator.GetEvent<AsyncJobFinishedEvent>().Publish(new AsyncJobFinishedEventArgs(this));
                },
                e =>
                {
                    WindowManager.CloseMessage();
                    finished = true;
                    if (error != null) error.Invoke(e);
                    EventAggregator.GetEvent<AsyncJobFinishedEvent>().Publish(new AsyncJobFinishedEventArgs(this));
                });
        }

        private void PopulationError(Exception ex)
        {
            if (ex is IOException || ex is UnauthorizedAccessException)
            {
                WindowManager.ShowMessage(Resx.IOError, ex.Message);
            }
            else
            {
                WindowManager.ShowErrorMessage(new SomethingWentWrongException(string.Format(Resx.PopulationFailed, ex.Message), ex));    
            }
        }

        private string RemoteChangeDirectory(string path)
        {
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            Telnet.ChangeFtpDirectory(dir);
            return path.Replace(dir, string.Empty);
        }

        private readonly Dictionary<Type, IFileListPaneViewModel> _paneCache = new Dictionary<Type, IFileListPaneViewModel>();

        private T Pane<T>() where T : class, IFileListPaneViewModel
        {
            var type = typeof (T);
            if (_paneCache.ContainsKey(type)) return (T)_paneCache[type];
            var pane = SourcePane as T ?? TargetPane as T;
            _paneCache.Add(type, pane);
            return pane;
        }

    }
}