using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Extensions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class IsoContentViewModel : FileListPaneViewModelBase<IsoContent>, IProgressViewModel
    {
       
        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override bool IsFavoritesSupported
        {
            get { return false; }
        }

        public override bool IsFSD
        {
            get { return false; }
        }

        public override bool IsVerificationEnabled
        {
            get { return false; }
        }

        private const string PROGRESSDIALOGTITLE = "ProgressDialogTitle";
        private readonly string _progressDialogTitle = Resx.IsoToGodConversion + " ({0}%)";
        public string ProgressDialogTitle
        {
            get { return string.Format(_progressDialogTitle, ProgressValue); }
        }

        private const string PROGRESSMESSAGE = "ProgressMessage";
        private string _progressMessage;
        public string ProgressMessage
        {
            get { return _progressMessage; }
            private set { _progressMessage = value; NotifyPropertyChanged(PROGRESSMESSAGE); }
        }

        private const string PROGRESSVALUE = "ProgressValue";
        private int _progressValue;
        public int ProgressValue
        {
            get { return _progressValue; }
            private set
            {
                _progressValue = value; 
                NotifyPropertyChanged(PROGRESSVALUE);
                NotifyPropertyChanged(PROGRESSVALUEDOUBLE);
                NotifyPropertyChanged(PROGRESSDIALOGTITLE);
            }
        }

        private const string PROGRESSVALUEDOUBLE = "ProgressValueDouble";
        public double ProgressValueDouble
        {
            get { return (double)ProgressValue / 100; }
        }

        private const string ISINDETERMINE = "IsIndetermine";
        private bool _isIndetermine;
        public bool IsIndetermine
        {
            get { return _isIndetermine; }
            private set { _isIndetermine = value; NotifyPropertyChanged(ISINDETERMINE); }
        }

        #region CloseCommand

        private void ExecuteCloseCommand()
        {
            EventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, null));
            Dispose();
        }

        #endregion

        public IsoContentViewModel()
        {
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            FileManager.ProgressChanged += FileManagerOnProgressChanged;
        }

        public override void LoadDataAsync(LoadCommand cmd, LoadDataAsyncParameters cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                case LoadCommand.Extract:
                case LoadCommand.Convert:
                    WorkHandler.Run(
                        () =>
                            {
                                FileManager.Load((string)cmdParam.Payload);
                                return true;
                            },
                        result =>
                            {
                                IsLoaded = true;
                                Initialize();
                                Drive = Drives.First();
                                if (success != null) success.Invoke(this);
                            },
                        exception =>
                            {
                                if (error != null) error.Invoke(this, exception);
                            });
                    break;
            }
        }

        protected override void ChangeDrive()
        {
            base.ChangeDrive();
            DriveLabel = FileManager.Details.Name ?? Resx.UnknownProfile;
        }

        protected override FileSystemItemViewModel GetDriveFromPath(string path)
        {
            return Drive;
        }

        public override string GetTargetPath(string path)
        {
            return string.Format("{0}{1}", CurrentFolder.Path, path.Replace('\\', '/'));
        }

        public override IList<FileSystemItem> GetList(string selectedPath)
        {
            //NOTE: to intentionaly turn off title recognition
            return FileManager.GetList(selectedPath);
        }

        public void ConvertToGod(string targetPath)
        {
            var viewModel = new GodConversionSettingsViewModel(targetPath, FileManager.Details, WindowManager);
            if (!WindowManager.ShowGodConversionSettingsDialog(viewModel)) return;

            ProgressMessage = Resx.StartingIsoConversion + Strings.DotDotDot;
            ProgressValue = 0;
            this.NotifyProgressStarted();

            WorkHandler.Run(() => ConvertToGodAsync(viewModel), ConvertToGodSuccess, ConvertToGodError);
        }

        private bool ConvertToGodAsync(GodConversionSettingsViewModel settingsViewModel)
        {
            if (settingsViewModel.RebuildType.Value == IsoRebuildType.Full)
            {
                FileManager.Rebuild(settingsViewModel.TempPath, settingsViewModel.SkipSystemUpdate);
            }

            // settingsViewModel.RebuildType == IsoRebuildType.Partial
            FileManager.ConvertToGod(settingsViewModel.TargetPath, settingsViewModel.Name, false);
            return true;
        }

        private void ConvertToGodSuccess(bool result)
        {
            this.NotifyProgressFinished();
        }

        private void ConvertToGodError(Exception ex)
        {
            this.NotifyProgressFinished();
            WindowManager.ShowMessage(Resx.Error, ex.Message);
        }

        private void FileManagerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UIThread.BeginRun(() =>
            {
                var message = e.UserState as string;
                if (message != null) ProgressMessage = message;
                if (e.ProgressPercentage != -1) ProgressValue = e.ProgressPercentage;
            });
        }

        public override void Dispose()
        {
            FileManager.ProgressChanged -= FileManagerOnProgressChanged;
            FileManager.Dispose();
            base.Dispose();
        }
    }
}