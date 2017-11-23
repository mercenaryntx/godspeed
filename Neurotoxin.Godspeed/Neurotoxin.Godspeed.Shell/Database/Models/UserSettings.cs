using System;
using System.ComponentModel;
using Neurotoxin.Godspeed.Shell.ViewModels;
using ServiceStack.DataAnnotations;

namespace Neurotoxin.Godspeed.Shell.Database.Models
{
    public class UserSettings : ModelBase
    {
        internal static int LatestVersion = 3;

        [Index]
        [PrimaryKey]
        public int Id { get; set; }

        private Guid _clientId;
        public Guid ClientId
        {
            get { return _clientId; }
            set
            {
                _clientId = value;
                SetDirtyFlag("ClientId");
            }
        }

        private int _version;
        public int Version
        {
            get { return _version; }
            set
            {
                _version = value;
                SetDirtyFlag("Version");
            }
        }

        private string _language;

        [StringLength(5)]
        public string Language
        {
            get { return _language; }
            set
            {
                _language = value;
                SetDirtyFlag("Language");
            }
        }

        private bool _disableCustomChrome;

        public bool DisableCustomChrome
        {
            get { return _disableCustomChrome; }
            set
            {
                _disableCustomChrome = value;
                SetDirtyFlag("DisableCustomChrome");
            }
        }

        private bool _disableNotificationSound;

        public bool DisableNotificationSound
        {
            get { return _disableNotificationSound; }
            set
            {
                _disableNotificationSound = value;
                SetDirtyFlag("DisableNotificationSound");
            }
        }

        private bool _useVersionChecker;

        public bool UseVersionChecker
        {
            get { return _useVersionChecker; }
            set
            {
                _useVersionChecker = value;
                SetDirtyFlag("UseVersionChecker");
            }
        }

        private bool _verifyFileHashAfterFtpUpload;

        public bool VerifyFileHashAfterFtpUpload
        {
            get { return _verifyFileHashAfterFtpUpload; }
            set
            {
                _verifyFileHashAfterFtpUpload = value;
                SetDirtyFlag("VerifyFileHashAfterFtpUpload");
            }
        }

        private int _fsdContentScanTrigger;

        public int FsdContentScanTrigger
        {
            get { return _fsdContentScanTrigger; }
            set
            {
                _fsdContentScanTrigger = value;
                SetDirtyFlag("FsdContentScanTrigger");
            }
        }

        private bool _useRemoteCopy;

        public bool UseRemoteCopy
        {
            get { return _useRemoteCopy; }
            set
            {
                _useRemoteCopy = value;
                SetDirtyFlag("UseRemoteCopy");
            }
        }

        private bool _useUnity;

        public bool UseUnity
        {
            get { return _useUnity; }
            set
            {
                _useUnity = value;
                SetDirtyFlag("UseUnity");
            }
        }

        private int _profileExpiration;

        public int ProfileExpiration
        {
            get { return _profileExpiration; }
            set
            {
                _profileExpiration = value;
                SetDirtyFlag("ProfileExpiration");
            }
        }

        private bool _profileInvalidation;

        public bool ProfileInvalidation
        {
            get { return _profileInvalidation; }
            set
            {
                _profileInvalidation = value;
                SetDirtyFlag("ProfileInvalidation");
            }
        }

        private int _recognizedGameExpiration;

        public int RecognizedGameExpiration
        {
            get { return _recognizedGameExpiration; }
            set
            {
                _recognizedGameExpiration = value;
                SetDirtyFlag("RecognizedGameExpiration");
            }
        }

        private int _partiallyRecognizedGameExpiration;

        public int PartiallyRecognizedGameExpiration
        {
            get { return _partiallyRecognizedGameExpiration; }
            set
            {
                _partiallyRecognizedGameExpiration = value;
                SetDirtyFlag("PartiallyRecognizedGameExpiration");
            }
        }

        private int _unrecognizedGameExpiration;

        public int UnrecognizedGameExpiration
        {
            get { return _unrecognizedGameExpiration; }
            set
            {
                _unrecognizedGameExpiration = value;
                SetDirtyFlag("UnrecognizedGameExpiration");
            }
        }

        private int _xboxLiveContentExpiration;

        public int XboxLiveContentExpiration
        {
            get { return _xboxLiveContentExpiration; }
            set
            {
                _xboxLiveContentExpiration = value;
                SetDirtyFlag("XboxLiveContentExpiration");
            }
        }

        private bool _xboxLiveContentInvalidation;

        public bool XboxLiveContentInvalidation
        {
            get { return _xboxLiveContentInvalidation; }
            set
            {
                _xboxLiveContentInvalidation = value;
                SetDirtyFlag("XboxLiveContentInvalidation");
            }
        }

        private int _unknownContentExpiration;

        public int UnknownContentExpiration
        {
            get { return _unknownContentExpiration; }
            set
            {
                _unknownContentExpiration = value;
                SetDirtyFlag("UnknownContentExpiration");
            }
        }

        private string _leftPaneType;

        [StringLength(1000)]
        public string LeftPaneType
        {
            get { return _leftPaneType; }
            set
            {
                _leftPaneType = value;
                SetDirtyFlag("LeftPaneType");
            }
        }

        private string _leftPaneDirectory;

        public string LeftPaneDirectory
        {
            get { return _leftPaneDirectory; }
            set
            {
                _leftPaneDirectory = value;
                SetDirtyFlag("LeftPaneDirectory");
            }
        }

        private string _leftPaneSortByField;

        [StringLength(30)]
        public string LeftPaneSortByField
        {
            get { return _leftPaneSortByField; }
            set
            {
                _leftPaneSortByField = value;
                SetDirtyFlag("LeftPaneSortByField");
            }
        }

        private int _leftPaneSortDirection;

        public int LeftPaneSortDirection
        {
            get { return _leftPaneSortDirection; }
            set
            {
                _leftPaneSortDirection = value;
                SetDirtyFlag("LeftPaneSortDirection");
            }
        }

        private string _rightPaneType;

        [StringLength(1000)]
        public string RightPaneType
        {
            get { return _rightPaneType; }
            set
            {
                _rightPaneType = value;
                SetDirtyFlag("RightPaneType");
            }
        }

        private string _rightPaneDirectory;

        public string RightPaneDirectory
        {
            get { return _rightPaneDirectory; }
            set
            {
                _rightPaneDirectory = value;
                SetDirtyFlag("RightPaneDirectory");
            }
        }

        private string _rightPaneSortByField;

        [StringLength(30)]
        public string RightPaneSortByField
        {
            get { return _rightPaneSortByField; }
            set
            {
                _rightPaneSortByField = value;
                SetDirtyFlag("RightPaneSortByField");
            }
        }

        private int _rightPaneSortDirection;

        public int RightPaneSortDirection
        {
            get { return _rightPaneSortDirection; }
            set
            {
                _rightPaneSortDirection = value;
                SetDirtyFlag("RightPaneSortDirection");
            }
        }

        private bool? _disableUserStatisticsParticipation;

        public bool? DisableUserStatisticsParticipation
        {
            get { return _disableUserStatisticsParticipation; }
            set
            {
                _disableUserStatisticsParticipation = value;
                SetDirtyFlag("DisableUserStatisticsParticipation");
            }
        }

        private int _leftPaneDisplayColumnMode;

        public int LeftPaneDisplayColumnMode
        {
            get { return _leftPaneDisplayColumnMode; }
            set
            {
                _leftPaneDisplayColumnMode = value;
                SetDirtyFlag("LeftPaneDisplayColumnMode");
            }
        }

        private int _rightPaneDisplayColumnMode;

        public int RightPaneDisplayColumnMode
        {
            get { return _rightPaneDisplayColumnMode; }
            set
            {
                _rightPaneDisplayColumnMode = value;
                SetDirtyFlag("RightPaneDisplayColumnMode");
            }
        }

        private int _leftPaneViewMode;

        public int LeftPaneViewMode
        {
            get { return _leftPaneViewMode; }
            set
            {
                _leftPaneViewMode = value;
                SetDirtyFlag("LeftPaneViewMode");
            }
        }

        private int _rightPaneViewMode;

        public int RightPaneViewMode
        {
            get { return _rightPaneViewMode; }
            set
            {
                _rightPaneViewMode = value;
                SetDirtyFlag("RightPaneViewMode");
            }
        }

        private bool _disableFsdStatusPolling;

        public bool DisableFsdStatusPolling
        {
            get { return _disableFsdStatusPolling; }
            set
            {
                _disableFsdStatusPolling = value;
                SetDirtyFlag("DisableFsdStatusPolling");
            }
        }

        public UserSettings()
        {
            ClientId = Guid.NewGuid();
            Version = LatestVersion;
            UseVersionChecker = true;
            FsdContentScanTrigger = (int) Constants.FsdContentScanTrigger.AfterUpload;

            UseUnity = true;
            ProfileInvalidation = true;
            PartiallyRecognizedGameExpiration = 7;
            UnrecognizedGameExpiration = 7;
            XboxLiveContentExpiration = 14;
            XboxLiveContentInvalidation = true;

            LeftPaneType = typeof (LocalFileSystemContentViewModel).FullName;
            LeftPaneDirectory = @"C:\";
            LeftPaneSortByField = @"ComputedName";
            LeftPaneSortDirection = (int) ListSortDirection.Ascending;

            RightPaneType = typeof (ConnectionsViewModel).FullName;
            RightPaneDirectory = @"C:\";
            RightPaneSortByField = @"ComputedName";
            RightPaneSortDirection = (int) ListSortDirection.Ascending;
        }
    }
}