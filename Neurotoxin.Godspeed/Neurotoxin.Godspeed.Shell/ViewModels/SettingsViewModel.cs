using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using WPFLocalizeExtension.Engine;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class SettingsViewModel : CommonViewModelBase, ISettingsViewModel
    {
        private readonly IUserSettingsProvider _userSettingsProvider;
        private readonly ICacheManager _cacheManager;

        #region Content recognition

        public List<int> ExpirationTimeSpans { get; set; }

        private const string USEUNITY = "UseUnity";
        private bool _useUnity;
        public bool UseUnity
        {
            get { return _useUnity; }
            set { _useUnity = value; NotifyPropertyChanged(USEUNITY); }
        }

        private const string PROFILEEXPIRATION = "ProfileExpiration";
        private int _profileExpiration;
        public int ProfileExpiration
        {
            get { return _profileExpiration; }
            set { _profileExpiration = value; NotifyPropertyChanged(PROFILEEXPIRATION); }
        }

        private const string PROFILEINVALIDATION = "ProfileInvalidation";
        private bool _profileInvalidation;
        public bool ProfileInvalidation
        {
            get { return _profileInvalidation; }
            set { _profileInvalidation = value; NotifyPropertyChanged(PROFILEINVALIDATION); }
        }

        private const string RECOGNIZEDGAMEEXPIRATION = "RecognizedGameExpiration";
        private int _recognizedGameExpiration;
        public int RecognizedGameExpiration
        {
            get { return _recognizedGameExpiration; }
            set { _recognizedGameExpiration = value; NotifyPropertyChanged(RECOGNIZEDGAMEEXPIRATION); }
        }

        private const string PARTIALLYRECOGNIZEDGAMEEXPIRATION = "PartiallyRecognizedGameExpiration";
        private int _partiallyRecognizedGameExpiration;
        public int PartiallyRecognizedGameExpiration
        {
            get { return _partiallyRecognizedGameExpiration; }
            set { _partiallyRecognizedGameExpiration = value; NotifyPropertyChanged(PARTIALLYRECOGNIZEDGAMEEXPIRATION); }
        }

        private const string UNRECOGNIZEDGAMEEXPIRATION = "UnrecognizedGameExpiration";
        private int _unrecognizedGameExpiration;
        public int UnrecognizedGameExpiration
        {
            get { return _unrecognizedGameExpiration; }
            set { _unrecognizedGameExpiration = value; NotifyPropertyChanged(UNRECOGNIZEDGAMEEXPIRATION); }
        }

        private const string XBOXLIVECONTENTEXPIRATION = "XboxLiveContentExpiration";
        private int _xboxLiveContentExpiration;
        public int XboxLiveContentExpiration
        {
            get { return _xboxLiveContentExpiration; }
            set { _xboxLiveContentExpiration = value; NotifyPropertyChanged(XBOXLIVECONTENTEXPIRATION); }
        }

        private const string XBOXLIVECONTENTINVALIDATION = "XboxLiveContentInvalidation";
        private bool _xboxLiveContentInvalidation;
        public bool XboxLiveContentInvalidation
        {
            get { return _xboxLiveContentInvalidation; }
            set { _xboxLiveContentInvalidation = value; NotifyPropertyChanged(XBOXLIVECONTENTINVALIDATION); }
        }

        private const string UNKNOWNCONTENTEXPIRATION = "UnknownContentExpiration";
        private int _unknownContentExpiration;
        public int UnknownContentExpiration
        {
            get { return _unknownContentExpiration; }
            set { _unknownContentExpiration = value; NotifyPropertyChanged(UNKNOWNCONTENTEXPIRATION); }
        }

        #endregion

        #region Operation

        public List<FsdContentScanTrigger> FsdContentScanTriggerOptions { get; set; }

        private const string USEVERSIONCHECKER = "UseVersionChecker";
        private bool _useVersionChecker;
        public bool UseVersionChecker
        {
            get { return _useVersionChecker; }
            set { _useVersionChecker = value; NotifyPropertyChanged(USEVERSIONCHECKER); }
        }

        private const string DISABLENOTIFICATIONSOUND = "DisableNotificationSound";
        private bool _disableNotificationSound;
        public bool DisableNotificationSound
        {
            get { return _disableNotificationSound; }
            set { _disableNotificationSound = value; NotifyPropertyChanged(DISABLENOTIFICATIONSOUND); }
        }

        private const string DISABLEFSDSTATUSPOLLING = "DisableFsdStatusPolling";
        private bool _disableFsdStatusPolling;
        public bool DisableFsdStatusPolling
        {
            get { return _disableFsdStatusPolling; }
            set { _disableFsdStatusPolling = value; NotifyPropertyChanged(DISABLEFSDSTATUSPOLLING); }
        }

        private const string VERIFYFILEHASHAFTERFTPUPLOAD = "VerifyFileHashAfterFtpUpload";
        private bool _verifyFileHashAfterFtpUpload;
        public bool VerifyFileHashAfterFtpUpload
        {
            get { return _verifyFileHashAfterFtpUpload; }
            set { _verifyFileHashAfterFtpUpload = value; NotifyPropertyChanged(VERIFYFILEHASHAFTERFTPUPLOAD); }
        }

        private const string FSDCONTENTSCANTRIGGER = "FsdContentScanTrigger";
        private FsdContentScanTrigger _fsdContentScanTrigger;
        public FsdContentScanTrigger FsdContentScanTrigger
        {
            get { return _fsdContentScanTrigger; }
            set { _fsdContentScanTrigger = value; NotifyPropertyChanged(FSDCONTENTSCANTRIGGER); }
        }

        private const string USEREMOTECOPY = "UseRemoteCopy";
        private bool _useRemoteCopy;
        public bool UseRemoteCopy
        {
            get { return _useRemoteCopy; }
            set { _useRemoteCopy = value; NotifyPropertyChanged(USEREMOTECOPY); }
        }

        #endregion

        #region Appearance

        public List<CultureInfo> AvailableLanguages { get; set; }

        private const string LANGUAGE = "Language";
        private CultureInfo _language;
        public CultureInfo Language
        {
            get { return _language; }
            set { _language = value; NotifyPropertyChanged(LANGUAGE); }
        }

        private const string DISABLECUSTOMCHROME = "DisableCustomChrome";
        private bool _disableCustomChrome;
        public bool DisableCustomChrome
        {
            get { return _disableCustomChrome; }
            set { _disableCustomChrome = value; NotifyPropertyChanged(DISABLECUSTOMCHROME); }
        }

        #endregion

        #region ClearCacheCommand

        public DelegateCommand ClearCacheCommand { get; private set; }

        private void ExecuteClearCacheCommand()
        {
            WorkHandler.Run(() =>
                                 {
                                     WindowManager.ShowMessage(Resx.ApplicationIsBusy, Resx.PleaseWait, NotificationMessageFlags.NonClosable);
                                     _cacheManager.Clear();
                                     return true;
                                 }, 
                             r => WindowManager.CloseMessage());
        }

        #endregion

        public SettingsViewModel(IUserSettingsProvider userSettingsProvider, ICacheManager cacheManager)
        {
            _userSettingsProvider = userSettingsProvider;
            _cacheManager = cacheManager;
            ClearCacheCommand = new DelegateCommand(ExecuteClearCacheCommand);

            ExpirationTimeSpans = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 14, 21, 30, 60, 90 };
            FsdContentScanTriggerOptions = Enum.GetValues(typeof (FsdContentScanTrigger)).ToList<FsdContentScanTrigger>();
            AvailableLanguages = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c =>
            {
                try
                {
                    if (c.IsNeutralCulture) return false;
                    if (c.Equals(CultureInfo.InvariantCulture)) return false;
                    if (c.Name == "en-US") return true;
                    return Resx.ResourceManager.GetResourceSet(c, true, false) != null;
                }
                catch (CultureNotFoundException)
                {
                    return false;
                }
            }).ToList();

            UseUnity = _userSettingsProvider.UseUnity;
            ProfileExpiration = _userSettingsProvider.ProfileExpiration;
            ProfileInvalidation = _userSettingsProvider.ProfileInvalidation;
            RecognizedGameExpiration = _userSettingsProvider.RecognizedGameExpiration;
            PartiallyRecognizedGameExpiration = _userSettingsProvider.PartiallyRecognizedGameExpiration;
            UnrecognizedGameExpiration = _userSettingsProvider.UnrecognizedGameExpiration;
            XboxLiveContentExpiration = _userSettingsProvider.XboxLiveContentExpiration;
            XboxLiveContentInvalidation = _userSettingsProvider.XboxLiveContentInvalidation;
            UnknownContentExpiration = _userSettingsProvider.UnknownContentExpiration;
            UseVersionChecker = _userSettingsProvider.UseVersionChecker;
            DisableNotificationSound = _userSettingsProvider.DisableNotificationSound;
            DisableFsdStatusPolling = _userSettingsProvider.DisableFsdStatusPolling;
            VerifyFileHashAfterFtpUpload = _userSettingsProvider.VerifyFileHashAfterFtpUpload;
            FsdContentScanTrigger = _userSettingsProvider.FsdContentScanTrigger;
            UseRemoteCopy = _userSettingsProvider.UseRemoteCopy;
            Language = _userSettingsProvider.Language ?? LocalizeDictionary.Instance.Culture;
            DisableCustomChrome = _userSettingsProvider.DisableCustomChrome;
        }

        public void SaveChanges()
        {
            _userSettingsProvider.UseUnity = UseUnity;
            _userSettingsProvider.ProfileExpiration = ProfileExpiration;
            _userSettingsProvider.ProfileInvalidation = ProfileInvalidation;
            _userSettingsProvider.RecognizedGameExpiration = RecognizedGameExpiration;
            _userSettingsProvider.PartiallyRecognizedGameExpiration = PartiallyRecognizedGameExpiration;
            _userSettingsProvider.UnrecognizedGameExpiration = UnrecognizedGameExpiration;
            _userSettingsProvider.XboxLiveContentExpiration = XboxLiveContentExpiration;
            _userSettingsProvider.XboxLiveContentInvalidation = XboxLiveContentInvalidation;
            _userSettingsProvider.UnknownContentExpiration = UnknownContentExpiration;
            _userSettingsProvider.UseVersionChecker = UseVersionChecker;
            _userSettingsProvider.DisableNotificationSound = DisableNotificationSound;
            _userSettingsProvider.DisableFsdStatusPolling = DisableFsdStatusPolling;
            _userSettingsProvider.VerifyFileHashAfterFtpUpload = VerifyFileHashAfterFtpUpload;
            _userSettingsProvider.FsdContentScanTrigger = FsdContentScanTrigger;
            _userSettingsProvider.UseRemoteCopy = UseRemoteCopy;
            _userSettingsProvider.Language = Language;
            _userSettingsProvider.DisableCustomChrome = DisableCustomChrome;
            _userSettingsProvider.PersistData();
        }
    }
}