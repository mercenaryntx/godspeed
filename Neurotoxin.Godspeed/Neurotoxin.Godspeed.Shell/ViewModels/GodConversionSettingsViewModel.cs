using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using Neurotoxin.Godspeed.Core.Io.Iso;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.ViewModels;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class GodConversionSettingsViewModel : ViewModelBase
    {
        private readonly IWindowManager _windowManager;

        private const string TARGETPATH = "TargetPath";
        private string _targetPath;
        public string TargetPath
        {
            get { return _targetPath; }
            set { _targetPath = value; NotifyPropertyChanged(TARGETPATH); }
        }

        private const string TEMPPATH = "TempPath";
        private string _tempPath;
        public string TempPath
        {
            get { return _tempPath; }
            set { _tempPath = value; NotifyPropertyChanged(TEMPPATH); }
        }

        private const string SAVEREBUILTISOIMAGE = "SaveRebuiltIsoImage";
        private bool _saveRebuiltIsoImage;
        public bool SaveRebuiltIsoImage
        {
            get { return _saveRebuiltIsoImage; }
            set { _saveRebuiltIsoImage = value; NotifyPropertyChanged(SAVEREBUILTISOIMAGE); }
        }

        private const string SKIPSYSTEMUPTATE = "SkipSystemUpdate";
        private bool _skipSystemUpdate;
        public bool SkipSystemUpdate
        {
            get { return _skipSystemUpdate; }
            set { _skipSystemUpdate = value; NotifyPropertyChanged(SKIPSYSTEMUPTATE); }
        }

        private const string ISFULLREBUILD = "IsFullRebuild";
        public bool IsFullRebuild
        {
            get { return RebuildType.Value == IsoRebuildType.Full; }
        }

        public ObservableCollection<ComboBoxItemViewModel<IsoRebuildType>> RebuildTypeOptions { get; private set; }

        private const string REBUILDTYPE = "RebuildType";
        private ComboBoxItemViewModel<IsoRebuildType> _rebuildType;
        public ComboBoxItemViewModel<IsoRebuildType> RebuildType
        {
            get { return _rebuildType; }
            set
            {
                _rebuildType = value; 
                NotifyPropertyChanged(REBUILDTYPE);
                NotifyPropertyChanged(ISFULLREBUILD);
            }
        }

        private const string NAME = "Name";
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(NAME); }
        }

        public string MediaId { get; private set; }
        public string TitleId { get; private set; }
        public string Disc { get; private set; }
        public BitmapImage Thumbnail { get; private set; }

        #region BrowseCommand

        public DelegateCommand<string> BrowseCommand { get; private set; }

        private void ExecuteBrowseCommand(string propertyName)
        {
            var pi = GetType().GetProperty(propertyName);
            if (pi == null) return;
            var newValue = _windowManager.ShowFolderBrowserDialog((string)pi.GetValue(this, null), Resx.ResourceManager.GetString("FolderBrowserDescription" + propertyName));
            if (newValue == null) return;
            pi.SetValue(this, newValue, null);
        }

        #endregion

        public GodConversionSettingsViewModel(string targetPath, XisoDetails xiso, IWindowManager windowManager)
        {
            _windowManager = windowManager;
            TargetPath = targetPath;
            TempPath = targetPath;
            MediaId = xiso.MediaId;
            TitleId = xiso.TitleId;
            Disc = string.Format("{0}/{1}", xiso.DiscNumber, xiso.DiscCount);
            Name = xiso.Name;
            Thumbnail = xiso.Thumbnail == null || xiso.Thumbnail.Length == 0 || xiso.Thumbnail[0] == 0
                ? null
                : StfsPackageExtensions.GetBitmapFromByteArray(xiso.Thumbnail);

            BrowseCommand = new DelegateCommand<string>(ExecuteBrowseCommand);
            RebuildTypeOptions = Enum.GetValues(typeof(IsoRebuildType)).Cast<IsoRebuildType>().Select(t => new ComboBoxItemViewModel<IsoRebuildType>(t)).ToObservableCollection();

            //TODO: temporary turned off
            RebuildTypeOptions[1].IsSelectable = false;
            RebuildType = RebuildTypeOptions.First();

            SaveRebuiltIsoImage = true;
            SkipSystemUpdate = true;
        }
    }
}