using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Neurotoxin.Godspeed.Core.Io.Gpd;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class ProfileRebuilderViewModel : PaneViewModelBase
    {
        private BinaryContent _packageContent;
        private StfsPackage _stfs;

        #region Properties

        private const string TABS = "Tabs";
        public ObservableCollection<ProfileRebuilderTabItemViewModel> _tabs;
        public ObservableCollection<ProfileRebuilderTabItemViewModel> Tabs
        {
            get { return _tabs; }
            set { _tabs = value; NotifyPropertyChanged(TABS); }
        }

        private const string SELECTEDTAB = "SelectedTab";
        public ProfileRebuilderTabItemViewModel _selectedTab;
        public ProfileRebuilderTabItemViewModel SelectedTab
        {
            get { return _selectedTab; }
            set { _selectedTab = value; NotifyPropertyChanged(SELECTEDTAB); }
        }

        #endregion

        #region CloseCommand

        public DelegateCommand CloseCommand { get; private set; }

        private void ExecuteCloseCommand()
        {
            EventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, null));
        }

        #endregion

        #region OpenTabCommand

        public DelegateCommand<EventInformation<MouseEventArgs>> OpenTabCommand { get; private set; }

        private void ExecuteOpenTabCommand(EventInformation<MouseEventArgs> e)
        {
            var fileEntry = e.CommandArgument as FileEntryViewModel;
            if (fileEntry == null || fileEntry.Blocks.Any(b => b.Health != FileBlockHealthStatus.Ok)) return;

            var existing = Tabs.SingleOrDefault(t => t.Header == fileEntry.Name);
            if (existing != null)
            {
                SelectedTab = existing;
                return;
            }

            var stfs = fileEntry.Parent;

            if (fileEntry.Name == "PEC")
            {
                var newTab = new ProfileRebuilderTabItemViewModel(fileEntry.Name, ParseStfs(stfs.ExtractPec()));
                Tabs.Add(newTab);
                SelectedTab = newTab;
            }
            else if (fileEntry.Name.EndsWith(".gpd"))
            {
                var gpd = stfs.ExtractFile(fileEntry.Name);
                var model = ModelFactory.GetModel<GpdFile>(gpd);
                model.Parse();
                var newTab = new ProfileRebuilderTabItemViewModel(fileEntry.Name, new GpdFileViewModel(model));
                Tabs.Add(newTab);
                SelectedTab = newTab;
            }
        }

        #endregion

        #region CloseTabCommand

        public DelegateCommand<ProfileRebuilderTabItemViewModel> CloseTabCommand { get; private set; }

        private void ExecuteCloseTabCommand(ProfileRebuilderTabItemViewModel e)
        {
            var index = Tabs.IndexOf(e);
            if (index == 0) return;
            Tabs.Remove(e);
            SelectedTab = Tabs[index-1];
        }

        #endregion

        public ProfileRebuilderViewModel()
        {
            Tabs = new ObservableCollection<ProfileRebuilderTabItemViewModel>();
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            OpenTabCommand = new DelegateCommand<EventInformation<MouseEventArgs>>(ExecuteOpenTabCommand);
            CloseTabCommand = new DelegateCommand<ProfileRebuilderTabItemViewModel>(ExecuteCloseTabCommand);
        }

        public override void Refresh(bool refreshCache)
        {
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
                            _stfs = ModelFactory.GetModel<StfsPackage>(_packageContent.Content);
                            return true;
                        },
                        result =>
                            {
                                IsLoaded = true;
                                Tabs.Add(new ProfileRebuilderTabItemViewModel(Resx.FileStructure, ParseStfs(_stfs)));
                                SelectedTab = Tabs.First();
                                if (success != null) success.Invoke(this);
                            },
                        exception =>
                        {
                            if (error != null) error.Invoke(this, exception);
                        });
                    break;
            }
        }

        private static ObservableCollection<FileEntryViewModel> ParseStfs(StfsPackage package)
        {
            var collection = new ObservableCollection<FileEntryViewModel>();
            var allocatedBlocks = new HashSet<int>();
            var blockCollisions = new HashSet<int>();
            foreach (var fileEntry in package.FlatFileList.Where(f => !f.IsDirectory).OrderBy(f => f.Name))
            {
                var blockList = package.GetFileEntryBlockList(fileEntry);
                foreach (var block in blockList.Where(b => b.Key.HasValue))
                {
                    if (!allocatedBlocks.Contains(block.Key.Value))
                        allocatedBlocks.Add(block.Key.Value);
                    else
                        blockCollisions.Add(block.Key.Value);
                }
                collection.Add(new FileEntryViewModel(fileEntry, blockList, package));
            }

            foreach (var block in blockCollisions.SelectMany(blockCollision => collection.SelectMany(vm => vm.Blocks.Where(b => b.BlockNumber == blockCollision))))
            {
                block.Health = FileBlockHealthStatus.Collision;
            }

            return collection;
        }

    }
}