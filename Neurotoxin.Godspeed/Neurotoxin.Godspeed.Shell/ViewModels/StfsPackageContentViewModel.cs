using System;
using System.Linq;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class StfsPackageContentViewModel : FileListPaneViewModelBase<StfsPackageContent>
    {
        private BinaryContent _packageContent;

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

        #region SaveAndCloseCommand

        public DelegateCommand SaveAndCloseCommand { get; private set; }

        private void ExecuteSaveAndCloseCommand()
        {
            _packageContent.Content = FileManager.Save();
            EventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, _packageContent));
            Dispose();
        }

        #endregion

        #region CloseCommand

        private void ExecuteCloseCommand()
        {
            EventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, null));
            Dispose();
        }

        #endregion

        public StfsPackageContentViewModel()
        {
            SaveAndCloseCommand = new DelegateCommand(ExecuteSaveAndCloseCommand);
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
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
                                _packageContent = (BinaryContent)cmdParam.Payload;
                                FileManager.LoadPackage(_packageContent);
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
            try
            {
                DriveLabel = FileManager.GetAccount().GamerTag;
            }
            catch
            {
                DriveLabel = Resx.UnknownProfile;
            }
            base.ChangeDrive();
        }

        protected override FileSystemItemViewModel GetDriveFromPath(string path)
        {
            return Drive;
        }

        public override string GetTargetPath(string path)
        {
            return string.Format("{0}{1}", CurrentFolder.Path, path.Replace('\\', '/'));
        }

        public override void Dispose()
        {
            FileManager.Dispose();
            base.Dispose();
        }
    }
}