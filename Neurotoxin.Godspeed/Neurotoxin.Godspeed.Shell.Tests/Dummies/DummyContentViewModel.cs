using System;
using System.ComponentModel;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Tests.Dummies
{
    public class DummyContentViewModel : FileListPaneViewModelBase<DummyContent>
    {
        public DummyContentViewModel(FakingRules rules)
        {
            Settings = new FileListPaneSettings("/", "ComputedName", ListSortDirection.Ascending, ColumnMode.Title, FileListPaneViewMode.List);
            FileManager.FakingRules = rules;
            Initialize();
        }

        public override bool IsReadOnly
        {
            get { return false; }
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

        public override string GetTargetPath(string path)
        {
            return CurrentFolder.Path + path;
        }

        protected override FileSystemItemViewModel GetDriveFromPath(string path)
        {
            //TODO
            throw new NotImplementedException();
        }
    }
}