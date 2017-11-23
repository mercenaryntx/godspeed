using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Practices.Composite;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Io;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Extensions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class LocalFileSystemContentViewModel : FileListPaneViewModelBase<LocalFileSystemContent>
    {
        public bool IsNetworkDrive
        {
            get { return Drive.FullPath.StartsWith(@"\\"); }
        }

        public string SilentTargetPath { get; set; }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override bool IsFavoritesSupported
        {
            get { return true; }
        }

        public override bool IsFSD
        {
            get { return false; }
        }

        public override bool IsVerificationEnabled
        {
            get { return false; }
        }

        protected override void ExecuteChangeDirectoryCommand(object cmdParam)
        {
            if (CurrentRow != null && CurrentRow.Type == ItemType.File)
            {
                if (CurrentRow.IsCompressedFile) OpenCompressedFileCommand.Execute();
                else if (CurrentRow.IsIso) OpenIsoCommand.Execute(LoadCommand.Load);
                else if (CurrentRow.TitleType == TitleType.Profile) OpenStfsPackageCommand.Execute(OpenStfsPackageMode.Browsing);
                else ExecuteOpenWithExplorerCommand();
                return;
            }
            base.ExecuteChangeDirectoryCommand(cmdParam);
        }

        #region OpenWithExplorerCommand

        public DelegateCommand OpenWithExplorerCommand { get; private set; }

        private bool CanExecuteOpenWithExplorerCommand()
        {
            return true;
        }

        private void ExecuteOpenWithExplorerCommand()
        {
            switch (CurrentRow.Type)
            {
                case ItemType.Directory:
                case ItemType.File:
                    Process.Start(CurrentRow.Path);
                    break;
                case ItemType.Link:
                    ChangeDirectoryCommand.Execute(null);
                    break;
            }
        }

        #endregion

        #region OpenIsoCommand

        public DelegateCommand<LoadCommand> OpenIsoCommand { get; private set; }

        protected virtual bool CanExecuteOpenIsoCommand(LoadCommand cmdParam)
        {
            return CurrentRow != null && CurrentRow.IsIso;
        }

        private void ExecuteOpenIsoCommand(LoadCommand cmdParam)
        {
            ProgressMessage = string.Format("{0} {1}...", Resx.OpeningIso, CurrentRow.ComputedName);
            IsBusy = true;
            OpenIso(CurrentRow.Model.Path, cmdParam);
        }

        private void OpenIso(string path, LoadCommand cmdParam)
        {
            var iso = Container.Resolve<IsoContentViewModel>();
            iso.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(Settings.Clone("/"), path), p => OpenIsoSuccess(p, path, cmdParam), OpenIsoError);
        }

        private void OpenIsoSuccess(PaneViewModelBase pane, string path, LoadCommand cmdParam)
        {
            IsBusy = false;
            var isoContentViewModel = (IsoContentViewModel)pane;
            switch (cmdParam)
            {
                case LoadCommand.Load:
                    EventAggregator.GetEvent<OpenNestedPaneEvent>().Publish(new OpenNestedPaneEventArgs(this, pane));
                    break;
                case LoadCommand.Extract:
                    var targetPath = WindowManager.ShowFolderBrowserDialog(path, Resx.FolderBrowserDescriptionIsoExtract);
                    if (string.IsNullOrWhiteSpace(targetPath)) return;
                    SilentTargetPath = targetPath;
                    isoContentViewModel.SelectAllCommand.Execute(null);
                    EventAggregator.GetEvent<ExecuteFileOperationEvent>().Publish(new ExecuteFileOperationEventArgs(FileOperation.Copy, isoContentViewModel, this, null));
                    break;
                case LoadCommand.Convert:
                    isoContentViewModel.ConvertToGod(Path.GetDirectoryName(path));
                    break;
            }
        }

        private void OpenIsoError(PaneViewModelBase pane, Exception exception)
        {
            IsBusy = false;
            WindowManager.ShowMessage(Resx.OpenFailed, string.Format("{0}: {1}", string.Format(Resx.CantOpenFile, CurrentRow.ComputedName), exception.Message));
        }

        #endregion

        public LocalFileSystemContentViewModel()
        {
            EventAggregator.GetEvent<UsbDeviceChangedEvent>().Subscribe(OnUsbDeviceChanged);
            OpenWithExplorerCommand = new DelegateCommand(ExecuteOpenWithExplorerCommand, CanExecuteOpenWithExplorerCommand);
            OpenIsoCommand = new DelegateCommand<LoadCommand>(ExecuteOpenIsoCommand, CanExecuteOpenIsoCommand);

            EventAggregator.GetEvent<TransferFinishedEvent>().Subscribe(OnTransferFinished);
        }

        private void OnTransferFinished(TransferFinishedEventArgs e)
        {
            SilentTargetPath = null;
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            if (OpenIsoCommand != null) OpenIsoCommand.RaiseCanExecuteChanged();
            if (OpenWithExplorerCommand != null) OpenWithExplorerCommand.RaiseCanExecuteChanged();
        }

        public override void Refresh(bool refreshCache)
        {
            if (string.IsNullOrEmpty(SilentTargetPath)) base.Refresh(refreshCache);
        }

        public override void LoadDataAsync(LoadCommand cmd, LoadDataAsyncParameters cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                    Initialize();
                    SetFavorites();
                    var drive = GetDriveFromPath(Settings.Directory);
                    if (drive != null) PathCache.Add(drive, Settings.Directory);
                    Drive = drive ?? GetDefaultDrive();
                    break;
                case LoadCommand.Restore:
                    var payload = cmdParam.Payload as BinaryContent;
                    if (payload == null) return;
                    WorkHandler.Run(
                        () =>
                        {
                            File.WriteAllBytes(payload.FilePath, payload.Content);
                            return true;
                        },
                        result =>
                        {
                            if (success != null) success.Invoke(this);
                        },
                        exception =>
                        {
                            if (error != null) error.Invoke(this, exception);
                        });
                    break;
            }
        }

        private FileSystemItemViewModel GetDefaultDrive()
        {
            if (Drives.Count == 0) return null;
            return Drives.FirstOrDefault(d => d.Name == "C:") ?? Drives.First();
        }


        protected override void ChangeDrive()
        {
            base.ChangeDrive();
            UpdateDriveInfo();
        }

        protected override FileSystemItemViewModel GetDriveFromPath(string path)
        {
            var storedDrive = Path.GetPathRoot(path);
            return Drives.FirstOrDefault(d => d.Path == storedDrive);
        }

        protected override void ChangeDirectory(string message = null, Action callback = null, bool refreshCache = false)
        {
            if (CurrentFolder.Type == ItemType.Link)
            {
                var reparsePoint = new ReparsePoint(CurrentFolder.Path);
                var path = reparsePoint.Target;
                if (path == null)
                {
                    if (reparsePoint.LastError == 5)
                    {
                        try
                        {
                            var cmd = string.Format("/k dir \"{0}\" /AL", Path.Combine(CurrentFolder.Path, ".."));
                            var p = Process.Start(new ProcessStartInfo("cmd.exe", cmd)
                            {
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false
                            });
                            var r = new Regex(string.Format("{0} \\[(.*?)\\]", Regex.Escape(CurrentFolder.Name)), RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            string line;
                            while ((line = p.StandardOutput.ReadLine()) != null)
                            {
                                var m = r.Match(line);
                                if (!m.Success) continue;
                                path = m.Groups[1].Value;
                                break;
                            }
                            p.Close();
                        }
                        catch
                        {
                            //do nothing if something goes wrong
                        }
                    }

                    if (string.IsNullOrEmpty(path))
                    {
                        WindowManager.ShowMessage(Resx.IOError, reparsePoint.LastError == 5 ? Resx.ReparsePointCannotBeAccessed : Resx.ReparsePointCannotBeResolved);
                        return;
                    }
                }
                var model = FileManager.GetItemInfo(path, ItemType.Directory);
                if (model == null)
                {
                    WindowManager.ShowMessage(Resx.IOError, string.Format(Resx.ItemNotExistsOnPath, path));
                    return;
                }
                CurrentFolder = new FileSystemItemViewModel(model);
            }
            base.ChangeDirectory(message, callback, refreshCache);
        }

        protected override void ChangeDirectoryCallback(IList<FileSystemItem> result, bool refreshCache)
        {
            base.ChangeDirectoryCallback(result, refreshCache);
            UpdateDriveInfo();
        }

        private void UpdateDriveInfo()
        {
            var driveInfo = DriveInfo.GetDrives().First(d => d.Name == Drive.Path);
            DriveLabel = string.Format("[{0}]", string.IsNullOrEmpty(driveInfo.VolumeLabel) ? "_NONE_" : driveInfo.VolumeLabel);
            FreeSpaceText = String.Format(Resx.LocalFileSystemFreeSpace, driveInfo.AvailableFreeSpace, driveInfo.TotalSize);
        }

        public override string GetTargetPath(string path)
        {
            return Path.Combine(SilentTargetPath ?? CurrentFolder.Path, path.Replace('/', '\\'));
        }

        private void OnUsbDeviceChanged(UsbDeviceChangedEventArgs e)
        {
            var drives = FileManager.GetDrives();
            for (var i = 0; i < drives.Count; i++)
            {
                var current = drives[i];
                if (i < Drives.Count && current.Name == Drives[i].Name) continue;
                if (i < Drives.Count && Drives.Any(d => d.Name == current.Name))
                {
                    var existing = Drives[i];
                    if (Drive.Name == existing.Name)
                        Drive = GetDefaultDrive();
                    Drives.Remove(existing);
                } 
                else
                {
                    Drives.Insert(i, new FileSystemItemViewModel(current));
                }
            }
        }

        public override void Dispose()
        {
            EventAggregator.GetEvent<UsbDeviceChangedEvent>().Unsubscribe(OnUsbDeviceChanged);
            base.Dispose();
        }
    }
}